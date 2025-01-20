using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_csharp.Migrations
{
    /// <inheritdoc />
    public partial class AddedAllTablesPlusRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.CreateTable(
                name: "DatabaseSong",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseSong", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Playlist",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Playlist_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LastListenedSong",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DatabaseSongId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LastListenedSong", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LastListenedSong_DatabaseSong_DatabaseSongId",
                        column: x => x.DatabaseSongId,
                        principalTable: "DatabaseSong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LastListenedSong_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseSongPlaylist",
                columns: table => new
                {
                    PlaylistsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SongsId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseSongPlaylist", x => new { x.PlaylistsId, x.SongsId });
                    table.ForeignKey(
                        name: "FK_DatabaseSongPlaylist_DatabaseSong_SongsId",
                        column: x => x.SongsId,
                        principalTable: "DatabaseSong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DatabaseSongPlaylist_Playlist_PlaylistsId",
                        column: x => x.PlaylistsId,
                        principalTable: "Playlist",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseSongPlaylist_SongsId",
                table: "DatabaseSongPlaylist",
                column: "SongsId");

            migrationBuilder.CreateIndex(
                name: "IX_LastListenedSong_DatabaseSongId",
                table: "LastListenedSong",
                column: "DatabaseSongId");

            migrationBuilder.CreateIndex(
                name: "IX_LastListenedSong_UserId",
                table: "LastListenedSong",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlist_UserId",
                table: "Playlist",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DatabaseSongPlaylist");

            migrationBuilder.DropTable(
                name: "LastListenedSong");

            migrationBuilder.DropTable(
                name: "Playlist");

            migrationBuilder.DropTable(
                name: "DatabaseSong");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT")
                .Annotation("Sqlite:Autoincrement", true);
        }
    }
}
