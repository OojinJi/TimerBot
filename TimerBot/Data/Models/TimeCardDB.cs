using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TimerBot.Data.Models;

[Table("TimeCard")]
public partial class TimeCardDB
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("USER_NAME")]
    public string? Name { get; set; }

    [Column("USER_ID")]
    public long User_Id{ get; set; }

    [Column("SERVER_ID")]
    public long Server_Id { get; set; }

    [Column("START_TIME")]
    public DateTime Start_Time { get; set; }    
    [Column("CHANNEL_ID")]
    public long Channel_Id { get; set; }
    [Column("Type")]
    public int type { get; set; } = 1;
}
