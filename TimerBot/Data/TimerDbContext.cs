using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TimerBot.Data.Models;

namespace TimerBot.Data;

public partial class TimerDbContext : DbContext
{
    public TimerDbContext()
    {
    }

    public TimerDbContext(DbContextOptions<TimerDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Server> Servers { get; set; }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<TimeCardDB> TimeCards { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=OOJIND\\DISCORDBOTSERVER;Initial Catalog=TimerBot;Integrated Security=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Server>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });
        modelBuilder.Entity<TimeCardDB>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TimeCard");
        });
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_USER_1");

            entity.Property(e => e.AverageTime).IsFixedLength();

            entity.HasOne(d => d.Server).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SERVER_ID_FK");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
