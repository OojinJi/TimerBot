using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimerBot.Data;
using TimerBot.Data.Models;
using TimerBot.Models;


namespace TimerBot.Services
{
    public class DataService
    {

        public async Task<bool> serverExist(SocketGuild guild)
        {
            using(var db = new TimerDbContext())
            {
                return (db.Servers.Where(x => x.Id == (long)guild.Id).FirstOrDefault() != null);
            }
        }
        public async Task<bool> userExist(SocketGuild guild, SocketUser user)
        {
            using (var db = new TimerDbContext())
            {
                return (db.Users.Where(x => x.UserId == (long)user.Id && x.ServerId == (long)guild.Id).FirstOrDefault() != null);
            }
        }

        public async Task addTimeCard(TimeCard timeCard)
        {
            using(var db = new TimerDbContext())
            {
                db.TimeCards.Add(new TimeCardDB
                {
                    User_Id = long.Parse(timeCard.userId),
                    Server_Id = (long)timeCard.guildId,
                    Start_Time = timeCard.startTime,
                    Channel_Id = (long)timeCard.channelId
                });
                db.SaveChanges();
            }
        }
        public async Task<List<TimeCard>> getTimeCard()
        {
            using (var db = new TimerDbContext())
            {
                List<TimeCardDB> timeCardDB = db.TimeCards.Where(x => x.type == 1).ToList();
                List<TimeCard> timeCardList = new List<TimeCard>();
                foreach (var timeCard in timeCardDB)
                {
                    timeCardList.Add(new TimeCard
                    {
                        userId = timeCard.User_Id.ToString(),
                        guildId = (ulong)timeCard.Server_Id,
                        startTime = timeCard.Start_Time,
                        channelId = (ulong)timeCard.Channel_Id
                    });
                }
                return timeCardList;
            }
        }

        public async Task delTimeCard(TimeCard card)
        {
            using (var db = new TimerDbContext())
            {
                db.TimeCards.Remove(db.TimeCards.FirstOrDefault(x => x.User_Id == long.Parse(card.userId) && x.Server_Id == (long)card.guildId && x.type == 1));
                db.SaveChanges();
            }
        }

        public async Task addBotChannel(long gId, long cId)
        {
            using (var db = new TimerDbContext())
            {
                Server server = db.Servers.Where(x => x.Id == gId).FirstOrDefault();
                server.botChannel = cId;
                db.SaveChanges();
            }
        } 

        public async Task<long> getBotChannelId(long gId)
        {
            using (var db = new TimerDbContext())
            {
                Server server = db.Servers.Where(x => x.Id == gId).FirstOrDefault() ;
                if (server != null)
                {
                    return server.botChannel;
                }
                return 0;
                
            }
        }

        public async Task delServer(SocketGuild guild)
        {
            using (var db = new TimerDbContext())
            {
                List<User> _users = db.Users.Where(x => (ulong)x.ServerId == guild.Id).ToList();
                foreach (User _user in _users)
                {
                    db.Users.Remove(_user);
                }
                db.Servers.Remove(db.Servers.Where(x => x.Id == (long)guild.Id).FirstOrDefault());
                db.SaveChanges();
            }
        }

        public async Task<List<double>> Leaderboard(SocketGuild guild, SocketUser user, int number)
        {
            using (var db = new TimerDbContext())
            {
                User? _user = db.Users.Where(x => x.UserId == (long)user.Id && x.ServerId == (long)guild.Id).FirstOrDefault();
                List<double> times = new List<double>();
                if (_user != null)
                {
                    foreach (string t in _user.Time.Split(','))
                    {
                        times.Add(double.Parse(t));
                    }
                    times.Sort((a, b) => b.CompareTo(a));
                    return times.Take(number).ToList();
                }
                return null;
                
            }
        }
        public async Task<List<TimeCard>> Leaderboard(SocketGuild guild, int number)
        {
            using (var db = new TimerDbContext())
            {
           
                List<TimeCard> times = new List<TimeCard>();
                foreach(var u in guild.Users)
                {
                    User _user = db.Users.Where(x => x.UserId == (long)u.Id && x.ServerId == (long)guild.Id).FirstOrDefault();

                    if(_user != null)
                    {
                        foreach (string t in _user.Time.Split(','))
                        {
                            times.Add(new TimeCard()
                            {
                                time = double.Parse(t),
                                userId = _user.UserId.ToString(),

                            });
                        }
                    }
                    
                }
                times.Sort((a, b) => b.time.CompareTo(a.time));
                return times.Take(number).ToList();
            }
        }

        public async Task<string> getpredition(SocketGuild guild, SocketUser user)
        {
            using (var db = new TimerDbContext())
            {
                User _user = db.Users.Where(x => x.UserId == (long)user.Id && x.ServerId == (long)guild.Id).FirstOrDefault();
                if(_user.TimesRecorded < 5)
                {
                    return null;
                }
                TimeSpan diff = TimeSpan.FromMilliseconds(double.Parse(_user.AverageTime));
                double pred = 100 / (1 + Math.Pow(Math.E, (double)((-_user.TimesRecorded + 24.3) * 0.34)));
                string userName = (user as SocketGuildUser).Nickname ?? user.Username;
                string timeObj = diff.Hours.ToString() + "h " + diff.Minutes.ToString() + "m " + diff.Seconds.ToString() + "." + diff.Milliseconds.ToString() + "s";
                string outp = "I predict it will take " + userName  + " " + timeObj + " to join with a confidence of " + pred.ToString() + "%";
                return outp;
            }
        }

        public async Task<userModel> getUserModel(SocketGuild guild, SocketUser user)
        {
            using(var db = new TimerDbContext())
            {
                User? existingUser = db.Users.Where(x => x.UserId == (long)user.Id && x.ServerId == (long)guild.Id).FirstOrDefault();
                return new userModel
                {
                    Id = (long)existingUser.UserId,
                    Name = existingUser.Name,
                    AverageTime = existingUser.AverageTime,
                    ServerId = existingUser.ServerId,
                    Time = existingUser.Time,
                    Times_Recorded = existingUser.TimesRecorded
                    
                };
            }
        }
        public void insertNewServer(serverModel server)
        {
            using (var db = new TimerDbContext())
            {
                Server? existingServer = db.Servers.Where(x => x.Id == server.Id).FirstOrDefault();
                if (existingServer == null)
                {
                    db.Servers.Add(new Server { Id = server.Id, Name = server.Name });
                    Console.WriteLine("Added Server " + server.Name + " " + server.Id);
                }
                else
                {
                    Console.WriteLine("Server already Exists");
                }

                Console.WriteLine("Adding Users");
                db.SaveChanges();
                foreach (userModel u in server.users)
                {
                    insertUser(u);
                }
                db.SaveChanges();

            }
        }

        public async Task<string> getUserName(long u, ulong g)
        {
            using(var db = new TimerDbContext())
            {
                return db.Users.FirstOrDefault(x => x.UserId == u && x.ServerId == (long)g).Name;
            }
        }


        public void insertUser(userModel user)
        {
            using (var db = new TimerDbContext())
            {
                User? existingUser = db.Users.Where(x => x.UserId == user.Id && x.ServerId == user.ServerId).FirstOrDefault();
                if (existingUser == null)
                {
                    db.Users.Add(new User()
                    {
                        UserId = user.Id,
                        Name = user.Name,
                        ServerId = user.ServerId,
                        Time = "0",
                        AverageTime = "0",
                        TimesRecorded = 0
                    });
                    Console.WriteLine("Added user " + user.Name + " to " + user.ServerId);
                }
                else
                {
                    Console.WriteLine(user.Name + " already here updating time");
                    existingUser.Name = user.Name; 
                    existingUser.Time = user.Time;
                    existingUser.AverageTime = user.AverageTime;
                    existingUser.TimesRecorded += 1;
                }
                db.SaveChanges();
            }
        }
    }
}
