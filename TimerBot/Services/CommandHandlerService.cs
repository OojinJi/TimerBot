using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TimerBot.Services
{
    public class CommandHandlerService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly DataService _dataService;
        private readonly IServiceProvider _services;

        public CommandHandlerService(IServiceProvider services, DataService dataService)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _dataService = dataService; 
            _services = services;

            _commands.CommandExecuted += CommandExecutedAsync;
            _discord.MessageReceived += MessageRecivedAsync;

        }
         public async Task InitializerAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        }

        public async Task MessageRecivedAsync(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message))
                return;
            if (message.Source != MessageSource.User)
                return;

            var argPos = 0;

            if (!message.HasMentionPrefix(_discord.CurrentUser, ref argPos))
                return;

            var context = new SocketCommandContext(_discord, message);
            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified)
                return;
            if (result.IsSuccess)
                return;
            await context.Channel.SendMessageAsync($"error: {result}");
        }

    }
}
