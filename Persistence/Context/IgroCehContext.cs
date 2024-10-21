using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Context
{
    public class IgroCehContext: DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<EventRecord> EventRecord { get; set; }

        public IgroCehContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
                .HasMany(u => u.Guilds)
                .WithMany(g => g.Users);
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Guild)
                .WithMany(g => g.Events)
                .HasForeignKey(e => e.GuildId);
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Creator)
                .WithMany(c => c.CreatorOfEvents)
                .HasForeignKey(e => e.CreatorId);
            modelBuilder.Entity<EventRecord>()
                .HasOne(er => er.Event)
                .WithMany(e => e.EventRecords)
                .HasForeignKey(er => er.EventId);
            modelBuilder.Entity<EventRecord>()
                .HasOne(er => er.FromUser)
                .WithMany()
                .HasForeignKey(er => er.FromUserId);
            modelBuilder.Entity<EventRecord>()
                .HasOne(er => er.ToUser)
                .WithMany()
                .HasForeignKey(er => er.ToUserId);
            modelBuilder.Entity<EventRecord>()
                .HasOne(er => er.Game)
                .WithMany(g => g.EventRecords)
                .HasForeignKey(er => er.GameId);
        }
    }
}
