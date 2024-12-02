
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TimerBot.Data;
using TimerBot.Services;
namespace TimerBot
{
    class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            using (var services = ConfigureServices())
            {
                var client = services.GetRequiredService<DiscordSocketClient>();
                client.Log += LogAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;
                await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("TimerBotToken", EnvironmentVariableTarget.Machine));
                await client.StartAsync();
                await services.GetRequiredService<CommandHandlerService>().InitializerAsync();
                services.GetRequiredService<VoiceEventService>();
                services.GetRequiredService<CommonService>();
                await client.SetGameAsync("the clock", null, ActivityType.Watching);
                await Task.Delay(Timeout.Infinite);
            }
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            //add external log save
            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
               .AddSingleton(new DiscordSocketConfig
               {
                   GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers,
                   LogLevel = LogSeverity.Debug,
                   AlwaysDownloadUsers = true,
                   MessageCacheSize = 100
               })
               .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlerService>()
                .AddSingleton<EventHandler>()
                .AddSingleton<HttpClient>()
                .AddSingleton<TimeCardService>()
                .AddSingleton<VoiceEventService>()
                .AddSingleton<CommonService>()
                .AddSingleton<TimerDbContext>()
                .AddSingleton<DataService>()
                .BuildServiceProvider();
        }
    }
}