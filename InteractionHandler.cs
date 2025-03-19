using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using InteractionFramework.Jsons;
using InteractionFramework.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace InteractionFramework
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _handler;
        private readonly IServiceProvider _services;

        private Thread _parseThread, _deliveryThread, _syncWithServerThread;
        private const string MAIN_PARSE_LINK = "https://gameinfo.albiononline.com/api/gameinfo/events?limit=51&offset=0";

        private List<KillEvent> killEvents = new List<KillEvent>();
        private List<int> completedEvents = new List<int>();

        private Config config = ConfigHandler.Source.config;

        public InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services)
        {
            _client = client;
            _handler = handler;
            _services = services;
        }

        public async Task InitializeAsync()
        {
            _client.Ready += ReadyAsync;
            _client.InteractionCreated += HandleInteraction;
            _client.UserVoiceStateUpdated += UserVoiceStateUpdated;
            _client.JoinedGuild += BotJoinedGuild;
            _handler.Log += LogAsync;

            await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task ReadyAsync()
        {
            //REGISTER COMMAND
            await _handler.RegisterCommandsGloballyAsync(true);

            //STATUS
            await _client.SetStatusAsync(UserStatus.Online);
            await _client.SetGameAsync("https://deatheye.cc");

            #region DELIVERY THREAD 1 SEC DELAY

            _deliveryThread = new Thread(() =>
            {
                while (true)
                {
                    if (killEvents.Count() != 0)
                    {
                        for (int i = 0; i < killEvents.Count(); i++)
                        {
                            Dictionary<ulong, GuildSetting> dictionary = ConfigHandler.Source.GetData();

                            foreach (KeyValuePair<ulong, GuildSetting> key in dictionary)
                            {
                                if (key.Value.KillBotSettings == null) continue;

                                IEnumerable<SocketGuild> server = _client.Guilds.Where(x => x.Id == key.Key);
                                if (server == null) continue;

                                foreach (KeyValuePair<ulong, KillBotSetting> settings in key.Value.KillBotSettings)
                                {
                                    SocketTextChannel channel = server.First().GetTextChannel(settings.Key);

                                    if (channel == null)
                                    {
                                        config.GuildSettings[key.Key].KillBotSettings.Remove(settings.Key);
                                        continue;
                                    }

                                    if (config.GuildSettings[key.Key].Subscription < DateTime.Now)
                                    {
                                        if (config.GuildSettings[key.Key].FreeLimit == 0)
                                        {
                                            if (!config.GuildSettings[key.Key].LimitNotify)
                                            {
                                                config.GuildSettings[key.Key].LimitNotify = true;
                                                channel.SendMessageAsync("The free daily limit of 50 notifications has expired.");
                                            }

                                            continue;
                                        }
                                        else
                                        {
                                            config.GuildSettings[key.Key].FreeLimit -= 1;
                                        }
                                    }
                                    
                                    switch (settings.Value.Type)
                                    {
                                        case FilterType.Nickname:

                                            if (killEvents[i].Killer.Name.Equals(settings.Value.Condition, StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                OutputMessage.BroadcastMessage(channel, killEvents[i], "kill");
                                            }
                                            else if (killEvents[i].Victim.Name.Equals(settings.Value.Condition, StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                OutputMessage.BroadcastMessage(channel, killEvents[i], "died");
                                            }
                                            else if (killEvents[i].Participants.Any(x => x.Name.Equals(settings.Value.Condition, StringComparison.InvariantCultureIgnoreCase)))
                                            {
                                                OutputMessage.BroadcastMessage(channel, killEvents[i], "assist");
                                            }

                                            break;

                                        case FilterType.Guild:

                                            if (killEvents[i].Killer.GuildName.Equals(settings.Value.Condition, StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                OutputMessage.BroadcastMessage(channel, killEvents[i], "kill");
                                            }
                                            else if (killEvents[i].Victim.GuildName.Equals(settings.Value.Condition, StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                OutputMessage.BroadcastMessage(channel, killEvents[i], "died");
                                            }
                                            else if (killEvents[i].Participants.Any(x => x.GuildName.Equals(settings.Value.Condition, StringComparison.InvariantCultureIgnoreCase)))
                                            {
                                                OutputMessage.BroadcastMessage(channel, killEvents[i], "assist");
                                            }

                                            break;

                                        case FilterType.AllianceTag:

                                            if (killEvents[i].Killer.AllianceName.Equals(settings.Value.Condition, StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                OutputMessage.BroadcastMessage(channel, killEvents[i], "kill");
                                            }
                                            else if(killEvents[i].Victim.AllianceName.Equals(settings.Value.Condition, StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                OutputMessage.BroadcastMessage(channel, killEvents[i], "died");
                                            }
                                            else if (killEvents[i].Participants.Any(x=> x.AllianceName.Equals(settings.Value.Condition, StringComparison.InvariantCultureIgnoreCase)))
                                            {
                                                OutputMessage.BroadcastMessage(channel, killEvents[i], "assist");
                                            }

                                            break;

                                        case FilterType.All:
                                            OutputMessage.BroadcastMessage(channel, killEvents[i], "kill");
                                            break;
                                    }
                                }
                            }

                            completedEvents.Add(killEvents[i].EventId);
                            killEvents.RemoveAt(i);

                            Thread.Sleep(1000);
                        }
                    }

                    Thread.Sleep(2000);
                }
            });

            _deliveryThread.IsBackground = true;
            _deliveryThread.Start();

            #endregion

            #region PARSE THREAD 10 SEC DELAY

            _parseThread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            List<KillEvent> killEvents = JsonConvert.DeserializeObject<List<KillEvent>>(client.GetStringAsync(MAIN_PARSE_LINK).Result);

                            foreach (KillEvent killEvent in killEvents)
                            {
                                if (!completedEvents.Contains(killEvent.EventId))
                                {
                                    this.killEvents.Add(killEvent);
                                }
                            }
                        }
                    }
                    catch { }

                    Thread.Sleep(10000);
                }
            });

            _parseThread.IsBackground = true;
            _parseThread.Start();

            #endregion

            #region SYNC WITH SERVER 20 MIN DELAY

            _syncWithServerThread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(1200000);

                    completedEvents.RemoveRange(0, (int)(completedEvents.Count() * 0.8f));
                    ConfigHandler.Source.Save();
                }
            });

            _syncWithServerThread.IsBackground = true;
            _syncWithServerThread.Start();

            #endregion
        }

        private async Task BotJoinedGuild(SocketGuild guild)
        {
            if (!config.GuildSettings.ContainsKey(guild.Id))
            {
                config.GuildSettings.Add(guild.Id, new GuildSetting { FreeLimit = 50, Subscription = DateTime.MinValue, KillBotSettings = null, MasterVoiceSetting = null });
                ConfigHandler.Source.Save();
            }
        }

        private async Task UserVoiceStateUpdated(SocketUser socketUser, SocketVoiceState svs, SocketVoiceState svs2)
        {
            IGuildUser user = socketUser as IGuildUser;

            if (config.GuildSettings.ContainsKey(user.GuildId))
            {
                if (config.GuildSettings[user.GuildId].MasterVoiceSetting != null)
                {
                    if (svs.VoiceChannel != null)
                    {
                        if (svs.VoiceChannel.CategoryId == config.GuildSettings[user.GuildId].MasterVoiceSetting.CategoryId && svs.VoiceChannel.Id != config.GuildSettings[user.GuildId].MasterVoiceSetting.MasterVoiceChannelId)
                        {
                            await svs.VoiceChannel.DeleteAsync();
                        }
                    }

                    if (svs2.VoiceChannel != null)
                    {
                        if (svs2.VoiceChannel.Id == config.GuildSettings[user.GuildId].MasterVoiceSetting.MasterVoiceChannelId)
                        {
                            IVoiceChannel channel = await user.Guild.CreateVoiceChannelAsync($"{user.DisplayName}'s | channel", tcp => tcp.CategoryId = config.GuildSettings[user.GuildId].MasterVoiceSetting.CategoryId);

                            await channel.AddPermissionOverwriteAsync(user, OverwritePermissions.AllowAll(channel));
                            await user.ModifyAsync(x => { x.ChannelId = channel.Id; });
                        }
                    }
                }
            }
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                SocketInteractionContext context = new SocketInteractionContext(_client, interaction);
                IResult result = await _handler.ExecuteCommandAsync(context, _services);

                if (!result.IsSuccess)
                    switch (result.Error)
                    {
                        case InteractionCommandError.UnmetPrecondition:
                            // implement
                            break;
                        default:
                            break;
                    }
            }
            catch
            {
                if (interaction.Type is InteractionType.ApplicationCommand)
                    await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }

        private async Task LogAsync(LogMessage log) => Console.WriteLine(log);
    }
}
