using Domain.Attributes;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Persistence.Context
{
    public class IgroCehContext: DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewSection> ReviewSections { get; set; }
        public DbSet<UserGuild> UserGuilds { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<EventRecord> EventRecords { get; set; }
        public DbSet<EventStatus> EventStatuses { get; set; }
        public DbSet<PreferenceTag> PreferenceTags { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }

        public IgroCehContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            //if((await Database.GetPendingMigrationsAsync()) != null)
            //{
            //    Database.Migrate();
            //}
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Game>()
                .HasOne(g => g.Creator)
                .WithMany()
                .HasForeignKey(g => g.CreatorId);
            modelBuilder.Entity<Game>()
                .HasIndex(g => g.VgcId)
                .IsUnique();
            modelBuilder.Entity<Game>()
                .Property(g => g.Platforms)
                .HasConversion(
                    platforms => PlatformsToJson(platforms),
                    json => PlatformsFromJson(json),
                    new ValueComparer<List<string>>(
                        (first, second) => first.SequenceEqual(second),
                        platforms => platforms.Aggregate(0, (hash, platform) => HashCode.Combine(hash, platform.GetHashCode())),
                        platforms => platforms.ToList()))
                .HasMaxLength(255);
            modelBuilder.Entity<User>()
                .HasMany(u => u.EventRecords)
                .WithOne(er => er.Participant)
                .HasForeignKey(er => er.ParticipantId);
            modelBuilder.Entity<Event>().Property(x => x.Id).ValueGeneratedOnAdd();
            // deleted events and their records are left out of every query, IgnoreQueryFilters() reaches them
            modelBuilder.Entity<Event>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<EventRecord>().HasQueryFilter(er => !er.Event.IsDeleted);
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Guild)
                .WithMany(g => g.Events)
                .HasForeignKey(e => e.GuildId);
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Creator)
                .WithMany(c => c.CreatorOfEvents)
                .HasForeignKey(e => e.CreatorId);
            modelBuilder.Entity<EventRecord>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<EventRecord>()
                .HasOne(er => er.Event)
                .WithMany(e => e.EventRecords)
                .HasForeignKey(er => er.EventId);
            modelBuilder.Entity<EventRecord>()
                .HasOne(er => er.Participant)
                .WithMany(u => u.EventRecords)
                .HasForeignKey(er => er.ParticipantId);
            modelBuilder.Entity<EventRecord>()
                .HasOne(er => er.ToUser)
                .WithMany()
                .HasForeignKey(er => er.ToUserId);
            modelBuilder.Entity<EventRecord>()
                .HasOne(er => er.Game)
                .WithMany(g => g.EventRecords)
                .HasForeignKey(er => er.GameId);
            // one review for a passed game
            modelBuilder.Entity<Review>()
                .HasOne(r => r.EventRecord)
                .WithOne(er => er.Review)
                .HasForeignKey<Review>(r => r.EventRecordId);
            modelBuilder.Entity<Review>().HasQueryFilter(r => !r.EventRecord.Event.IsDeleted);
            modelBuilder.Entity<ReviewSection>()
                .HasOne(s => s.Review)
                .WithMany(r => r.Sections)
                .HasForeignKey(s => s.ReviewId);
            modelBuilder.Entity<ReviewSection>().HasQueryFilter(s => !s.Review.EventRecord.Event.IsDeleted);
            modelBuilder.Entity<ReviewSection>().Property(s => s.Title).HasMaxLength(ReviewLimits.TitleMaxLength);
            modelBuilder.Entity<ReviewSection>().Property(s => s.Text).HasMaxLength(ReviewLimits.TextMaxLength);
            modelBuilder.Entity<ReviewSection>().Property(s => s.Reply).HasMaxLength(ReviewLimits.ReplyMaxLength);
            modelBuilder.Entity<UserGuild>()
                .HasKey(ug => new { ug.UserId, ug.GuildId });
            modelBuilder.Entity<PreferenceTag>().Property(t => t.Name).HasMaxLength(PreferenceLimits.NameMaxLength).IsRequired();
            modelBuilder.Entity<PreferenceTag>().Property(t => t.NameRu).HasMaxLength(PreferenceLimits.NameMaxLength).IsRequired();
            modelBuilder.Entity<PreferenceTag>().HasIndex(t => t.Name).IsUnique();
            modelBuilder.Entity<PreferenceTag>().HasIndex(t => t.NameRu).IsUnique();
            modelBuilder.Entity<PreferenceTag>().HasData(PreferenceTagSeed.Tags);
            // User has no navigation to its preferences: the base repository includes every navigation of a user it loads
            modelBuilder.Entity<UserPreference>()
                .HasKey(p => new { p.UserId, p.PreferenceTagId });
            modelBuilder.Entity<UserPreference>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId);
            modelBuilder.Entity<UserPreference>()
                .HasOne(p => p.PreferenceTag)
                .WithMany()
                .HasForeignKey(p => p.PreferenceTagId);
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

        // platforms are a short list that is only read together with its game, so it is kept as json in a single column
        private static string PlatformsToJson(List<string> platforms) => JsonSerializer.Serialize(platforms ?? new List<string>());

        private static List<string> PlatformsFromJson(string json) =>
            string.IsNullOrEmpty(json) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
    }
}
