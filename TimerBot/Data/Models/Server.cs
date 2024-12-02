using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TimerBot.Data.Models;

[Table("SERVER")]
public partial class Server
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("NAME")]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [Column("BOT_CHANNEL")]
    public long botChannel { get; set; }

    [InverseProperty("Server")]
    public virtual ICollection<User> Users { get; } = new List<User>();
}
