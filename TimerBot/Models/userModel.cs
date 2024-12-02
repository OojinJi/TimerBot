using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimerBot.Models
{
    public class userModel
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public long ServerId { get; set; }
        public string? Time { get; set; }
        public string? AverageTime { get; set; }
        public int? Times_Recorded { get; set; }
    }
}
