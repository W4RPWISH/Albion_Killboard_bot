using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace InteractionFramework
{
    public class Program
    {
        private readonly IServiceProvider _services;

        private readonly DiscordSocketConfig _socketConfig = new()
        {
            GatewayIntents = GatewayIntents.All,
            AlwaysDownloadUsers = true,
            UseInteractionSnowflakeDate = false
        };

        public Program()
        {
            _services = new ServiceCollection()
                .AddSingleton(_socketConfig)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<InteractionHandler>()
                .BuildServiceProvider();
        }

        static void Main(string[] args)
            => new Program().RunAsync()
                .GetAwaiter()
                .GetResult();

        public async Task RunAsync()
        {
            var client = _services.GetRequiredService<DiscordSocketClient>();

            client.Log += LogAsync;

            await _services.GetRequiredService<InteractionHandler>().InitializeAsync();

            await client.LoginAsync(TokenType.Bot, "Bot TOKEN");
            await client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

        private async Task LogAsync(LogMessage message) => Console.WriteLine(message.ToString());
    }
}
