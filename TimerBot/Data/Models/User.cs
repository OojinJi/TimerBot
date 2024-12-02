using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TimerBot.Data.Models;

[Table("USER")]
public partial class User
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("NAME")]
    public string Name { get; set; } = null!;

    [Column("SERVER_ID")]
    public long ServerId { get; set; }

    [Column("TIME", TypeName = "xml")]
    public string? Time { get; set; }

    [Column("AVERAGE_TIME")]
    [StringLength(100)]
    public string? AverageTime { get; set; }

    [Column("TIMES_RECORDED")]
    public int? TimesRecorded { get; set; }

    [Column("USER_ID")]
    public long? UserId { get; set; }

    [ForeignKey("ServerId")]
    [InverseProperty("Users")]
    public virtual Server Server { get; set; } = null!;
}
