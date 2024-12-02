using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimerBot.Models
{
    public class serverModel
    {
        public long Id{ get; set; }
        public string Name{ get; set; }
        public List<userModel> users { get; set; }
    }
}
