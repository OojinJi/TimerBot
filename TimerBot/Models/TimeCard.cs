using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimerBot.Models
{
    public class TimeCard
    {
        public DateTime startTime { get; set; }
        public string userId { get; set; }
        public ulong channelId { get; set; }
        public ulong guildId { get; set; }
        public double time { get; set; }
        public string userName { get; set; }
        public int type { get; set; } = 1;
    }
}
