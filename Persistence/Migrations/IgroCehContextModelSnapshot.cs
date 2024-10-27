﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations
{
    [DbContext(typeof(IgroCehContext))]
    partial class IgroCehContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("Domain.Entities.Event", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("CreatorId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<DateTimeOffset>("EndDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("GuildId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<DateTimeOffset>("StartDate")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.HasIndex("GuildId");

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

                    b.ToTable("EventRecord");
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

            modelBuilder.Entity("GuildUser", b =>
                {
                    b.Property<string>("GuildsId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("UsersId")
                        .HasColumnType("varchar(255)");

                    b.HasKey("GuildsId", "UsersId");

                    b.HasIndex("UsersId");

                    b.ToTable("GuildUser");
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

                    b.Navigation("Creator");

                    b.Navigation("Guild");
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

            modelBuilder.Entity("GuildUser", b =>
                {
                    b.HasOne("Domain.Entities.Guild", null)
                        .WithMany()
                        .HasForeignKey("GuildsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UsersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Entities.Event", b =>
                {
                    b.Navigation("EventRecords");
                });

            modelBuilder.Entity("Domain.Entities.Game", b =>
                {
                    b.Navigation("EventRecords");
                });

            modelBuilder.Entity("Domain.Entities.Guild", b =>
                {
                    b.Navigation("Events");
                });

            modelBuilder.Entity("Domain.Entities.User", b =>
                {
                    b.Navigation("CreatorOfEvents");
                });
#pragma warning restore 612, 618
        }
    }
}
