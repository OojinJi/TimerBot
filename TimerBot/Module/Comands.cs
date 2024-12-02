using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TimerBot.Data.Models;
using TimerBot.Models;
using TimerBot.Services;

namespace TimerBot.Module
{
    public class Comands : ModuleBase<SocketCommandContext>
    {
        private readonly TimeCardService _timeCardService;
        private readonly CommonService _commonService;
        private readonly DataService _dataService;

        public Comands(TimeCardService timeCardService, CommonService commonService, DataService dataService)
        {
            _timeCardService = timeCardService;
            _commonService = commonService;
            _dataService = dataService;
        }
        [Command("ping")]
        [Alias("pong", "hello")]
        public Task PingAsync()
            => ReplyAsync("pong!");

        [Command("force")]
        [Alias("commandload")]
        public async Task force()
        {
            SocketGuild guild = Context.Guild;
            List<userModel> _users = new List<userModel>();
            foreach (var rawUser in guild.Users)
            {
                _users.Add(new userModel()
                {
                    Id = (long)rawUser.Id,
                    Name = rawUser.Nickname ?? rawUser.Username,
                    ServerId = (long)guild.Id
                });
                Console.WriteLine("Added " + rawUser.Nickname ?? rawUser.Username + " into server raw");
            }
        }


        [Command("help")]
        [Alias("everything is on fire", "AHHHHHHHHHHHHHHH")]
        public async Task HelpAsync()
        {
            var embed = new EmbedBuilder()
            {
                Title = "Help"

            };
            embed.AddField("time",
                "   This is command starts the timer for each mentioned user \n" +
                "   To use this command simmply @Timebert time @user1 @user2 etc..\n");

            embed.AddField("leaderboard",
                "   This command shows the top N slowest times for a specific user \n   or for the entire server, \n" +
                "   If you leave N blank it will get the top 5 \n" +
                "   If you leave @user blank it will get top N times in ther server\n" +
                "   To use this comannd simply @Timebert leaderboard @user N");

            embed.AddField("setbotchannel",
                "   This is command sets the bot channel \n" +
                "   If no channel is given it will use the current one "+
                "   setting bot channel enables unmute timming which runs automaticly"+
                "   To use this command simmply @Timebert setbotchannel #channelName \n");

            embed.AddField("removebotchannel",
                "   This is command removes the bot channel \n" +
                "   To use this command simmply @Timebert removebotchannel");
               
            embed.AddField("getCards",
                "   This is command displays all unresolved cards \n" +
                "   To use this command simmply @TimebertgetCards");

            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("fun")]
        public async Task fun()
        {
            if(Context.User.Id == 603000858161971211)
            {

                var bill = new EmbedBuilder()
                {
                    Title = "Bill"
                };
                bill.Color = Color.Orange;
                var body = new EmbedBuilder()
                {
                    Title = "Body"
                };
                body.Color = Color.Teal;
                var feet = new EmbedBuilder()
                {
                    Title = "Feet"
                };
                feet.Color = Color.Orange;
                var hat = new EmbedBuilder()
                {
                    Title = "Hat"
                };
                hat.Color = Color.DarkOrange;

                await Context.Message.ReplyAsync(embed: bill.Build());
                await Context.Channel.SendMessageAsync(embed: body.Build());
                await Context.Channel.SendMessageAsync(embed: feet.Build());
                await Context.Channel.SendMessageAsync("Wait who are you?");
                await Context.Channel.SendMessageAsync("An embeded text stack?");
                await Context.Channel.SendMessageAsync(embed: hat.Build());
                await Context.Channel.SendMessageAsync(embed: bill.Build());
                await Context.Channel.SendMessageAsync(embed: body.Build());
                await Context.Channel.SendMessageAsync(embed: feet.Build());
                await Context.Channel.SendMessageAsync("PERRY THE EMBEDED TEXT STACK!!!!");
            }
            else
            {
                await Context.Message.ReplyAsync("Sorry only my owner can send this command");
            } 
        }

        [Command("getCards")]
        public async Task getCards()
        {
            SocketGuild guild = Context.Guild;
            List<TimeCard> cards = await _timeCardService.getAllCard(guild);
            if(cards.Count != 0)
            {
                foreach (var card in cards)
                {
                    SocketGuildUser curUSer = guild.GetUser(ulong.Parse(card.userId));
                    string userName = curUSer.Nickname ?? curUSer.Username;
                    var embed = new EmbedBuilder()
                    {
                        Title = "Time card for " + curUSer.Mention
                    };

                    embed.Description = "Created at " + card.startTime.ToString("HH:mm:ss tt") + " on " + card.startTime.ToString("dddd, dd MMMM");
                    await Context.Message.ReplyAsync(embed: embed.Build());
                }
            }
            else
            {
                var embed = new EmbedBuilder()
                {
                    Title = "No active time cards"
                };
                await Context.Message.ReplyAsync(embed: embed.Build());
            }
        }
        
        [Command("setBotChannel")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task setbotchannel(SocketChannel channel)
        {
            SocketGuild curGuild = Context.Guild;
            await _dataService.addBotChannel((long)Context.Guild.Id, (long)channel.Id);
           await Context.Message.ReplyAsync("Bot channel set to " + channel);
        }
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("setBotChannel")]
        public async Task setbotchannel()
        {
            SocketGuild curGuild = Context.Guild;
            if (!await _dataService.serverExist(curGuild))
            {
                Console.WriteLine("Running firstime setup for " + curGuild.Name);
                _commonService.firstTimeLoad(curGuild);
            }
            await _dataService.addBotChannel((long)Context.Guild.Id, (long)Context.Channel.Id);
            await Context.Message.ReplyAsync("Bot channel set to " + Context.Channel);
        }
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("removeBotChannel")]
        public async Task removebotchannel()
        {
            await _dataService.addBotChannel((long)Context.Guild.Id, (long)0);
            await Context.Message.ReplyAsync(Context.Channel + " removed as botchannel");
        }


        [Command("leaderboard")]
        [Summary("Gets the top N results for User")]
        public async Task LeaderBoard([Summary("This is the user: ")] SocketUser user, [Remainder][Summary("This is the number of records to be pulled, with 5 being the default")] string number = "5")
        {
            SocketGuild curGuild = Context.Guild;
            List<double> leaderBoard = await _dataService.Leaderboard(curGuild,  user, int.Parse(number));
            string des = "";
            int counter = 1;

            string userName = (user as SocketGuildUser).Nickname ?? user.Username;

            var embed = new EmbedBuilder()
            {
                Title = "Leaderboard for " + user.Mention
            };
            
            if(leaderBoard.Count > 0)
            {
                foreach (double l in leaderBoard)
                {
                    if (l != 0)
                    {
                        TimeSpan diff = TimeSpan.FromMilliseconds(l);

                        string timeObj = diff.Days.ToString() + "d " + diff.Hours.ToString() + "h " + diff.Minutes.ToString() + "m " + diff.Seconds.ToString() + "." + diff.Milliseconds.ToString() + "s";
                        des += (counter.ToString() + ". " + timeObj + "\n");
                        counter++;
                    }

                }
                if (des != "")
                {
                    embed.WithDescription(des);
                }
                else
                {
                    embed.WithDescription("Not enough info");
                }
            }
            await Context.Message.ReplyAsync(embed: embed.Build());
        }


        [Command("leaderboard")]
        [Summary("Gets the top N results for server you are currently in")]
        public async Task LeaderBoard([Remainder][Summary("This is the number of records to be pulled, with 5 being the default")] string number = "5")
        {
            SocketGuild curGuild = Context.Guild;
            List<TimeCard> leaderBoard = await _dataService.Leaderboard(curGuild, int.Parse(number));
            string des = "";
            int counter = 1;
            
            var embed = new EmbedBuilder()
            {
                Title = "Leaderboard for " + curGuild.Name
            };

            if (leaderBoard.Count > 0)
            {
                foreach (TimeCard l in leaderBoard)
                {
                    if (l.time != 0)
                    {
                        SocketGuildUser curUser = curGuild.GetUser(ulong.Parse(l.userId));
                        string userName = curUser.Nickname ?? curUser.Username;

                        TimeSpan diff = TimeSpan.FromMilliseconds(l.time);
                        string timeObj = diff.Days.ToString() + "d " + diff.Hours.ToString() + "h " + diff.Minutes.ToString() + "m " + diff.Seconds.ToString() + "." + diff.Milliseconds.ToString() + "s";
                        des += (counter.ToString() + ". " + curUser.Mention + " " + timeObj + "\n");
                        counter++;
                    }

                }
                if (des != "")
                {
                    embed.WithDescription(des);
                }
                else
                {
                    embed.WithDescription("Not enough data");
                }
                await Context.Message.ReplyAsync(embed: embed.Build());

            }
        }

        [Command("time")]
        [Alias("Time")]
        [Summary("Begins timing specified user(s)")]
        public async Task timeAsync([Summary("User(s)")]params SocketUser[] _users)
        {
            List<SocketUser> users = _users.ToList();
            foreach(SocketUser user in users)
            {
                var gUser = user as SocketGuildUser;
                
                string userName = gUser.Nickname ?? gUser.Username;
                if (Context.Guild.VoiceChannels.SingleOrDefault(X => X.ConnectedUsers.Any(u => u.Id == user.Id)) == null )
                {
                    SocketGuild curGuild = Context.Guild;
                   
                    if (!await _dataService.userExist(curGuild, user))
                    { 
                        _dataService.insertUser(new userModel
                        {
                            ServerId = (long)curGuild.Id,
                            Id = (long)user.Id,
                            Name = userName
                        });
                    }
                    string pred = await _dataService.getpredition(curGuild, user);
                    
                    TimeCard existingCard =  await _timeCardService.getCardById(user.Id.ToString(), curGuild.Id);
                    if (existingCard == null)
                    {
                        await Context.Message.ReplyAsync(user.Mention + " is now being timed by Timebert");
                        await _timeCardService.createCard(userName, user.Id.ToString(), DateTime.Now, Context.Channel.Id, Context.Guild.Id);
                        if (pred != null)
                        {
                            await Context.Channel.SendMessageAsync(pred);
                        }
                    }
                    else
                    {
                        await Context.Message.ReplyAsync("This person is already being timed");
                    }
                    

                }
                else
                {
                    await Context.Message.ReplyAsync("This user is already here!");
                }
            }
            
            
        }

       

    }
}
