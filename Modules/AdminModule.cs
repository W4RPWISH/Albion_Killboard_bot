using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using InteractionFramework.Jsons;
using InteractionFramework.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace InteractionFramework.Modules
{
    [RequireUserPermission(GuildPermission.Administrator)]
    public class AdminModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly uint maincolor = Convert.ToUInt32("ff0000", 16);

        private Config config = ConfigHandler.Source.config;

        public InteractionService Commands { get; set; }

        private InteractionHandler _handler;

        public AdminModule(InteractionHandler handler)
        {
            _handler = handler;
        }

        [SlashCommand("subscription", "Check bot subscription status")]
        public async Task Subscription()
        {
            await DeferAsync(ephemeral: true);

            if (config.GuildSettings[Context.Guild.Id].Subscription > DateTime.Now)
            {
                await FollowupAsync($"**Bot subscription type : Premium, to {config.GuildSettings[Context.Guild.Id].Subscription}**", ephemeral: true);
            }
            else
            {
                await FollowupAsync($"**Bot subscription type : Free**", ephemeral: true);
            }
        }

        [SlashCommand("bindchannel", "Bind channel for Broadcast")]
        public async Task BindChannel(FilterType filterType, string Condition)
        {
            await DeferAsync(ephemeral: true);

            if (config.GuildSettings.ContainsKey(Context.Guild.Id))
            {
                if (config.GuildSettings[Context.Guild.Id].KillBotSettings != null)
                {
                    config.GuildSettings[Context.Guild.Id].KillBotSettings.Add(Context.Channel.Id, new KillBotSetting() { Type = filterType, Condition = Condition });
                }
                else
                {
                    config.GuildSettings[Context.Guild.Id].KillBotSettings = new Dictionary<ulong, KillBotSetting> 
                    {
                        { Context.Channel.Id, new KillBotSetting() { Type = filterType, Condition = Condition } }
                    };
                }

                ConfigHandler.Source.Save();
            }

            await FollowupAsync("**Channel binded success!**", ephemeral: true);
        }

        [SlashCommand("unbind", "Unbind channel")]
        public async Task Unbind()
        {
            await DeferAsync(ephemeral: true);

            config.GuildSettings[Context.Guild.Id].KillBotSettings.Remove(Context.Channel.Id);
            ConfigHandler.Source.Save();

            await FollowupAsync("**Channel unbinded success!**", ephemeral: true);
        }

        [SlashCommand("mastervoice", "Create mastervoice channel")]
        public async Task CreateMasterVoice(SocketVoiceChannel voiceChannel, SocketCategoryChannel category)
        {
            await DeferAsync(ephemeral: true);

            config.GuildSettings[Context.Guild.Id].MasterVoiceSetting = new MasterVoiceSetting() { MasterVoiceChannelId = voiceChannel.Id, CategoryId = category.Id };
            ConfigHandler.Source.Save();

            await FollowupAsync($"**Mastervoice channel set to {voiceChannel.Mention}**", ephemeral: true);
        }

        [SlashCommand("premium", "Activate Premium status")]
        public async Task Premium(string ActivationKey)
        {
            await DeferAsync(ephemeral: true);

            if (ActivationKey.Length != 35)
            {
                await FollowupAsync("**Use correct key!**", ephemeral: true);
                return;
            }

            List<string> keys = File.ReadAllLines("keys.txt").ToList();

            if (keys.Contains(ActivationKey))
            {
                keys.Remove(ActivationKey);
                File.WriteAllLines("keys.txt", keys);

                config.GuildSettings[Context.Guild.Id].Subscription = config.GuildSettings[Context.Guild.Id].Subscription >= DateTime.Now ? config.GuildSettings[Context.Guild.Id].Subscription.AddDays(30) : DateTime.Now.AddDays(30);
                ConfigHandler.Source.Save();

                await FollowupAsync("**Premium activated Success!**", ephemeral: true);
            }
            else
            {
                await FollowupAsync("**Key not found!**", ephemeral: true);
            }
        }
    }
}
