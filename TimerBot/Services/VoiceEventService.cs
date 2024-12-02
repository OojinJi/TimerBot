using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimerBot.Models;

namespace TimerBot.Services
{
    public class VoiceEventService
    {
        private readonly IServiceProvider _services;
        private readonly TimeCardService _timeCardService;
        private readonly DiscordSocketClient _discord;
        private readonly CommonService _commonService;
        private readonly DataService _dataService;
        public VoiceEventService(IServiceProvider services)
        {
            _services = services;
            _timeCardService = services.GetService<TimeCardService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _commonService = services.GetService<CommonService>();
            _dataService = services.GetService<DataService>();
            _discord.UserVoiceStateUpdated += eventUserVoiceStateUpdatedAsync;
        }
        public async Task eventUserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState curVoiceState, SocketVoiceState nextVoiceState)
        {

            if (user is not SocketGuildUser guildUser)
            {
                return;
            }
            SocketGuild guild;
            if (curVoiceState.VoiceChannel == null)
            {
                guild = nextVoiceState.VoiceChannel.Guild ;
            }
            else
            {
                guild = curVoiceState.VoiceChannel.Guild;
            }
            long botChannel = await _dataService.getBotChannelId((long)guild.Id);
            if (botChannel != 0)
            {
                if (!curVoiceState.IsSelfMuted && nextVoiceState.IsSelfMuted)
                {
                    TimeCard existingCard = await _timeCardService.getCardById(user.Id.ToString(), guild.Id);
                    string pred = await _dataService.getpredition(guild, user);

                    if (existingCard == null)
                    {

                        if (_dataService.getBotChannelId((long)guild.Id) != null)
                        {
                            await _timeCardService.createCard((user as SocketGuildUser).Nickname ?? user.Username, user.Id.ToString(), DateTime.Now, (ulong)botChannel, guild.Id);
                        }

                        if (pred != null)
                        {
                            await guild.GetTextChannel((ulong)botChannel).SendMessageAsync(pred);
                        }
                    }
                    else
                    {
                        await guild.GetTextChannel((ulong)botChannel).SendMessageAsync("This person is already being timed");
                    }
                }
                if ((curVoiceState.IsSelfMuted && !nextVoiceState.IsSelfMuted))
                {
                    var userTimeCard = await _timeCardService.getCardById(user.Id.ToString(), guild.Id);
                    if (userTimeCard != null && (userTimeCard.guildId == guild.Id))
                    {
                        var start = userTimeCard.startTime;
                        var end = DateTime.Now;
                        var diff = end - start;

                        userModel _userModel = await _dataService.getUserModel(guild, user);
                        _userModel.Name = (user as SocketGuildUser).Nickname ?? user.Username;
                        List<string> usrTime = _userModel.Time.Split(',').ToList() ?? new List<string> { "0" };
                        float last = float.Parse(usrTime[usrTime.Count - 1]);


                        string timeObj = " it took you " + diff.Days.ToString() + "d " + diff.Hours.ToString() + "h " + diff.Minutes.ToString() + "m " + diff.Seconds.ToString() + "." + diff.Milliseconds.ToString() + "s";

                        var channel = await _discord.GetChannelAsync(userTimeCard.channelId) as ISocketMessageChannel;
                        if (diff.TotalDays > 2)
                        {
                            await channel.SendMessageAsync("<@" + userTimeCard.userId + "> Look who decided to join.");
                        }

                            usrTime.Add(diff.TotalMilliseconds.ToString());
                            if (!nextVoiceState.IsMuted && curVoiceState.VoiceChannel != null)
                            {
                                await channel.SendMessageAsync("<@" + userTimeCard.userId + ">" + timeObj + " to unmute!");
                            }
                            else
                            {
                                await channel.SendMessageAsync("<@" + userTimeCard.userId + ">" + timeObj + " to join!");
                            }



                            if (diff.TotalMilliseconds > last)
                            {
                                var lastTime = diff.TotalMilliseconds - last;
                                TimeSpan lastT = TimeSpan.FromMilliseconds(lastTime);
                                timeObj = lastT.Hours.ToString() + "h " + lastT.Minutes.ToString() + "m " + lastT.Seconds.ToString() + "." + diff.Milliseconds.ToString() + "s";
                                await channel.SendMessageAsync("Which was " + timeObj + " slower then last time, boooooooo");
                            }
                            else
                            {
                                var lastTime = last - diff.TotalMilliseconds;
                                TimeSpan lastT = TimeSpan.FromMilliseconds(lastTime);
                                timeObj = lastT.Days.ToString() + "d " + lastT.Hours.ToString() + "h " + lastT.Minutes.ToString() + "m " + lastT.Seconds.ToString() + "." + lastT.Milliseconds.ToString() + "s";
                                await channel.SendMessageAsync("Which was " + timeObj + " faster then last time!");
                            }


                        _userModel.Time = String.Join(",", usrTime);
                        _userModel.AverageTime = await _commonService.getAv(usrTime);

                        _dataService.insertUser(_userModel);

                        await _timeCardService.deleteCardById(userTimeCard.userId, userTimeCard.guildId);
                        await _dataService.delTimeCard(userTimeCard);
                        Console.WriteLine(diff.TotalSeconds);
                    }
                }
            }


            if ((curVoiceState.VoiceChannel == null && nextVoiceState.VoiceChannel != null))
            {
                var userTimeCard = await _timeCardService.getCardById(user.Id.ToString(), guild.Id);
                if (userTimeCard != null && (userTimeCard.guildId == guild.Id))
                {
                    var start = userTimeCard.startTime;
                    var end = DateTime.Now;
                    var diff = end - start;

                    userModel _userModel = await _dataService.getUserModel(guild, user);
                    _userModel.Name = (user as SocketGuildUser).Nickname ?? user.Username;
                    List<string> usrTime = _userModel.Time.Split(',').ToList() ?? new List<string> { "0" };
                    float last = float.Parse(usrTime[usrTime.Count - 1]);


                    string timeObj = " it took you " + diff.Days.ToString() + "d " + diff.Hours.ToString() + "h " + diff.Minutes.ToString() + "m " + diff.Seconds.ToString() + "." + diff.Milliseconds.ToString() + "s";

                    var channel = await _discord.GetChannelAsync(userTimeCard.channelId) as ISocketMessageChannel;
                    if (diff.TotalDays > 2 && !(user.Id == 314509625892536320 || user.Id == 750894826001661962))
                    {
                        await channel.SendMessageAsync("<@" + userTimeCard.userId + "> Look who decided to join.");
                        usrTime.Add(diff.TotalMilliseconds.ToString());
                    }
                    else
                    {
                        usrTime.Add(diff.TotalMilliseconds.ToString());
                        if (!nextVoiceState.IsMuted && curVoiceState.VoiceChannel != null)
                        {
                            await channel.SendMessageAsync("<@" + userTimeCard.userId + ">" + timeObj + " to unmute!");
                        }
                        else
                        {
                            await channel.SendMessageAsync("<@" + userTimeCard.userId + ">" + timeObj + " to join!");
                        }



                        if (diff.TotalMilliseconds > last)
                        {
                            var lastTime = diff.TotalMilliseconds - last;
                            TimeSpan lastT = TimeSpan.FromMilliseconds(lastTime);
                            timeObj = lastT.Hours.ToString() + "h " + lastT.Minutes.ToString() + "m " + lastT.Seconds.ToString() + "." + diff.Milliseconds.ToString() + "s";
                            await channel.SendMessageAsync("Which was " + timeObj + " slower then last time, boooooooo");
                        }
                        else
                        {
                            var lastTime = last - diff.TotalMilliseconds;
                            TimeSpan lastT = TimeSpan.FromMilliseconds(lastTime);
                            timeObj = lastT.Days.ToString() + "d " + lastT.Hours.ToString() + "h " + lastT.Minutes.ToString() + "m " + lastT.Seconds.ToString() + "." + diff.Milliseconds.ToString() + "s";
                            await channel.SendMessageAsync("Which was " + timeObj + " faster then last time!");
                        }

                    }
                    _userModel.Time = String.Join(",", usrTime);
                    _userModel.AverageTime = await _commonService.getAv(usrTime);

                    _dataService.insertUser(_userModel);

                    await _timeCardService.deleteCardById(userTimeCard.userId, userTimeCard.guildId);
                    await _dataService.delTimeCard(userTimeCard);
                    Console.WriteLine(diff.TotalSeconds);
                }


            }
        }

    }
}
