using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;
using TimerBot.Models;

namespace TimerBot.Services
{
    public class TimeCardService
    {
        private readonly DataService _dataService;
        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _discord;
        public TimeCardService(DataService dataService, IServiceProvider services)
        {
            _dataService = dataService;
            _services = services;
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _discord.Ready += getTimeCards;
        }

        
        public List<TimeCard> timeCards = new List<TimeCard>();
        public async Task getTimeCards()
        {
   
            timeCards.AddRange(await _dataService.getTimeCard());
        }

        public async Task createCard(string name, string Id, DateTime date, ulong _channelId, ulong _guildId)
        {
            TimeCard timeCard = new TimeCard
            {
                userId = Id,
                startTime = date,
                channelId = _channelId,
                guildId = _guildId,
                userName = name,
               
            };

            timeCards.Add(timeCard);
            await _dataService.addTimeCard(timeCard);

            
        }
        public async Task deleteCardById(string Id, ulong gId)
        {
            timeCards.Remove(timeCards.First(x => x.userId == Id && x.guildId == gId && x.type == 1));
        }
        public async Task<TimeCard> getCardById(string Id, ulong guild )
        {
            return timeCards.FirstOrDefault(x => x.userId == Id && x.guildId == guild && x.type == 1);
        }

        public async Task<List<TimeCard>> getAllCard(SocketGuild guild)
        {
            return timeCards.FindAll(x => x.guildId == guild.Id && x.type == 1).ToList();
        }
        

    }
}
