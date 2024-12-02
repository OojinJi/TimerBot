using Discord.Audio;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimerBot.Models;

namespace TimerBot.Services
{
    public class CommonService
    {
        private readonly DataService _dataService;
        private readonly DiscordSocketClient _discord;
        private Dictionary<ulong, IAudioClient> _connections = new Dictionary<ulong, IAudioClient>();
        public CommonService(DataService dataService, IServiceProvider services)
        {
            _dataService = dataService;
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _discord.JoinedGuild += firstTimeLoad;
            _discord.LeftGuild += goodByeService;
        }


        public async Task goodByeService(SocketGuild guild)
        {
            _dataService.delServer(guild);
        }

        public async Task<string> getAv(List<string> times)
        {
            float sum = 0;
            long len = 0;
            foreach (var t in times)
            {
                if(float.Parse(t) != 0)
                {
                    sum += float.Parse(t);
                    len += 1;
                }

            }
            return (sum/len).ToString();
        }

        public async Task firstTimeLoad(SocketGuild guild)
        {
            if (!await _dataService.serverExist(guild))
            {
                var rawServer = guild;

                Console.WriteLine(rawServer);
                List<userModel> _users = new List<userModel>();

                foreach (var rawUser in guild.Users)
                {
                    _users.Add(new userModel()
                    {
                        Id = (long)rawUser.Id,
                        Name = rawUser.Nickname ?? rawUser.Username,
                        ServerId = (long)rawServer.Id
                    });
                    Console.WriteLine("Added " + rawUser.Nickname ?? rawUser.Username + " into server raw");
                }

                serverModel server = new serverModel()
                {
                    Id = (long)rawServer.Id,
                    Name = rawServer.Name,
                    users = _users
                };

                _dataService.insertNewServer(server);
            }
        }
    }
}
