using Domain.Attributes;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Persistence.Context
{
    public class IgroCehContext: DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<UserGuild> UserGuilds { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<EventRecord> EventRecords { get; set; }
        public DbSet<EventStatus> EventStatuses { get; set; }

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
            modelBuilder.Entity<Event>().Property(x => x.Id).ValueGeneratedOnAdd();
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
            modelBuilder.Entity<UserGuild>()
                .HasKey(ug => new { ug.UserId, ug.GuildId });
            modelBuilder.Entity<EventStatus>()
                .Property(e => e.Id)
                .HasConversion<int>();
            modelBuilder.Entity<EventStatus>()
                .HasMany(es => es.Events)
                .WithOne(e => e.Status)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EventStatus>()
                .HasData(
                    Enum.GetValues(typeof(EventStatusId))
                    .Cast<EventStatusId>()
                    .Select(esId => new EventStatus
                    {
                        Id = esId,
                        Name = esId.ToString(),
                        UserFriendlyName = esId.GetType().GetMember(esId.ToString()).First().GetCustomAttribute<DisplayAttribute>()?.GetName() ?? "",
                        Order = esId.GetType().GetMember(esId.ToString()).First().GetCustomAttribute<OrderAttribute>()?.Order ?? 0,
                    })
                );
        }
    }
}
