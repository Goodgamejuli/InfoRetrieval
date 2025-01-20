﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using backend_csharp.Database;

#nullable disable

namespace backend_csharp.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20250118182008_AddedAllTablesPlusRelations")]
    partial class AddedAllTablesPlusRelations
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.1");

            modelBuilder.Entity("DatabaseSongPlaylist", b =>
                {
                    b.Property<Guid>("PlaylistsId")
                        .HasColumnType("TEXT");

                    b.Property<string>("SongsId")
                        .HasColumnType("TEXT");

                    b.HasKey("PlaylistsId", "SongsId");

                    b.HasIndex("SongsId");

                    b.ToTable("DatabaseSongPlaylist");
                });

            modelBuilder.Entity("backend_csharp.Models.Database.DatabaseSong", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("DatabaseSong");
                });

            modelBuilder.Entity("backend_csharp.Models.Database.LastListenedSong", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("DatabaseSongId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("DatabaseSongId");

                    b.HasIndex("UserId");

                    b.ToTable("LastListenedSong");
                });

            modelBuilder.Entity("backend_csharp.Models.Database.Playlist", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Playlist");
                });

            modelBuilder.Entity("backend_csharp.Models.Database.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("DatabaseSongPlaylist", b =>
                {
                    b.HasOne("backend_csharp.Models.Database.Playlist", null)
                        .WithMany()
                        .HasForeignKey("PlaylistsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("backend_csharp.Models.Database.DatabaseSong", null)
                        .WithMany()
                        .HasForeignKey("SongsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("backend_csharp.Models.Database.LastListenedSong", b =>
                {
                    b.HasOne("backend_csharp.Models.Database.DatabaseSong", "DatabaseSong")
                        .WithMany("LastListenedSongs")
                        .HasForeignKey("DatabaseSongId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("backend_csharp.Models.Database.User", "User")
                        .WithMany("LastListenedSong")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DatabaseSong");

                    b.Navigation("User");
                });

            modelBuilder.Entity("backend_csharp.Models.Database.Playlist", b =>
                {
                    b.HasOne("backend_csharp.Models.Database.User", "User")
                        .WithMany("Playlists")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("backend_csharp.Models.Database.DatabaseSong", b =>
                {
                    b.Navigation("LastListenedSongs");
                });

            modelBuilder.Entity("backend_csharp.Models.Database.User", b =>
                {
                    b.Navigation("LastListenedSong");

                    b.Navigation("Playlists");
                });
#pragma warning restore 612, 618
        }
    }
}
