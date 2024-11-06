﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations
{
    [DbContext(typeof(IgroCehContext))]
    [Migration("20241106222838_StatusIdOrderWasAdded")]
    partial class StatusIdOrderWasAdded
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("Domain.Entities.Event", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("CreatorId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("GuildId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("StatusId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.HasIndex("GuildId");

                    b.HasIndex("StatusId");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("Domain.Entities.EventRecord", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("EventId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("FromUserId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("GameId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ToUserId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("EventId");

                    b.HasIndex("FromUserId");

                    b.HasIndex("GameId");

                    b.HasIndex("ToUserId");

                    b.ToTable("EventRecords");
                });

            modelBuilder.Entity("Domain.Entities.EventStatus", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Order")
                        .HasColumnType("int");

                    b.Property<string>("UserFriendlyName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("EventStatuses");

                    b.HasData(
                        new
                        {
                            Id = 0,
                            Name = "PlayersRegistration",
                            Order = 3,
                            UserFriendlyName = "Players registration"
                        },
                        new
                        {
                            Id = 1,
                            Name = "Auction",
                            Order = 2,
                            UserFriendlyName = "Auction"
                        },
                        new
                        {
                            Id = 2,
                            Name = "Active",
                            Order = 1,
                            UserFriendlyName = "Active"
                        },
                        new
                        {
                            Id = 3,
                            Name = "Finished",
                            Order = 4,
                            UserFriendlyName = "Finished"
                        });
                });

            modelBuilder.Entity("Domain.Entities.Game", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("SteamUrl")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("Domain.Entities.Guild", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("AvatarUrl")
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("Domain.Entities.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("AvatarUrl")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Domain.Entities.UserGuild", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("GuildId")
                        .HasColumnType("varchar(255)");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("UserId", "GuildId");

                    b.HasIndex("GuildId");

                    b.ToTable("UserGuilds");
                });

            modelBuilder.Entity("Domain.Entities.Event", b =>
                {
                    b.HasOne("Domain.Entities.User", "Creator")
                        .WithMany("CreatorOfEvents")
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Guild", "Guild")
                        .WithMany("Events")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.EventStatus", "Status")
                        .WithMany("Events")
                        .HasForeignKey("StatusId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Creator");

                    b.Navigation("Guild");

                    b.Navigation("Status");
                });

            modelBuilder.Entity("Domain.Entities.EventRecord", b =>
                {
                    b.HasOne("Domain.Entities.Event", "Event")
                        .WithMany("EventRecords")
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.User", "FromUser")
                        .WithMany()
                        .HasForeignKey("FromUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Game", "Game")
                        .WithMany("EventRecords")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.User", "ToUser")
                        .WithMany()
                        .HasForeignKey("ToUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Event");

                    b.Navigation("FromUser");

                    b.Navigation("Game");

                    b.Navigation("ToUser");
                });

            modelBuilder.Entity("Domain.Entities.UserGuild", b =>
                {
                    b.HasOne("Domain.Entities.Guild", "Guild")
                        .WithMany("UserGuilds")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.User", "User")
                        .WithMany("UserGuilds")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Domain.Entities.Event", b =>
                {
                    b.Navigation("EventRecords");
                });

            modelBuilder.Entity("Domain.Entities.EventStatus", b =>
                {
                    b.Navigation("Events");
                });

            modelBuilder.Entity("Domain.Entities.Game", b =>
                {
                    b.Navigation("EventRecords");
                });

            modelBuilder.Entity("Domain.Entities.Guild", b =>
                {
                    b.Navigation("Events");

                    b.Navigation("UserGuilds");
                });

            modelBuilder.Entity("Domain.Entities.User", b =>
                {
                    b.Navigation("CreatorOfEvents");

                    b.Navigation("UserGuilds");
                });
#pragma warning restore 612, 618
        }
    }
}
