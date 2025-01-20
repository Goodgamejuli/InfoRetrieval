using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_csharp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DatabaseSongPlaylist_DatabaseSong_SongsId",
                table: "DatabaseSongPlaylist");

            migrationBuilder.DropForeignKey(
                name: "FK_DatabaseSongPlaylist_Playlist_PlaylistsId",
                table: "DatabaseSongPlaylist");

            migrationBuilder.DropForeignKey(
                name: "FK_LastListenedSong_DatabaseSong_DatabaseSongId",
                table: "LastListenedSong");

            migrationBuilder.DropForeignKey(
                name: "FK_LastListenedSong_Users_UserId",
                table: "LastListenedSong");

            migrationBuilder.DropForeignKey(
                name: "FK_Playlist_Users_UserId",
                table: "Playlist");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Playlist",
                table: "Playlist");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LastListenedSong",
                table: "LastListenedSong");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DatabaseSong",
                table: "DatabaseSong");

            migrationBuilder.RenameTable(
                name: "Playlist",
                newName: "Playlists");

            migrationBuilder.RenameTable(
                name: "LastListenedSong",
                newName: "LastListenedSongs");

            migrationBuilder.RenameTable(
                name: "DatabaseSong",
                newName: "DatabaseSongs");

            migrationBuilder.RenameIndex(
                name: "IX_Playlist_UserId",
                table: "Playlists",
                newName: "IX_Playlists_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_LastListenedSong_UserId",
                table: "LastListenedSongs",
                newName: "IX_LastListenedSongs_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_LastListenedSong_DatabaseSongId",
                table: "LastListenedSongs",
                newName: "IX_LastListenedSongs_DatabaseSongId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Playlists",
                table: "Playlists",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LastListenedSongs",
                table: "LastListenedSongs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DatabaseSongs",
                table: "DatabaseSongs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DatabaseSongPlaylist_DatabaseSongs_SongsId",
                table: "DatabaseSongPlaylist",
                column: "SongsId",
                principalTable: "DatabaseSongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DatabaseSongPlaylist_Playlists_PlaylistsId",
                table: "DatabaseSongPlaylist",
                column: "PlaylistsId",
                principalTable: "Playlists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LastListenedSongs_DatabaseSongs_DatabaseSongId",
                table: "LastListenedSongs",
                column: "DatabaseSongId",
                principalTable: "DatabaseSongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LastListenedSongs_Users_UserId",
                table: "LastListenedSongs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Playlists_Users_UserId",
                table: "Playlists",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DatabaseSongPlaylist_DatabaseSongs_SongsId",
                table: "DatabaseSongPlaylist");

            migrationBuilder.DropForeignKey(
                name: "FK_DatabaseSongPlaylist_Playlists_PlaylistsId",
                table: "DatabaseSongPlaylist");

            migrationBuilder.DropForeignKey(
                name: "FK_LastListenedSongs_DatabaseSongs_DatabaseSongId",
                table: "LastListenedSongs");

            migrationBuilder.DropForeignKey(
                name: "FK_LastListenedSongs_Users_UserId",
                table: "LastListenedSongs");

            migrationBuilder.DropForeignKey(
                name: "FK_Playlists_Users_UserId",
                table: "Playlists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Playlists",
                table: "Playlists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LastListenedSongs",
                table: "LastListenedSongs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DatabaseSongs",
                table: "DatabaseSongs");

            migrationBuilder.RenameTable(
                name: "Playlists",
                newName: "Playlist");

            migrationBuilder.RenameTable(
                name: "LastListenedSongs",
                newName: "LastListenedSong");

            migrationBuilder.RenameTable(
                name: "DatabaseSongs",
                newName: "DatabaseSong");

            migrationBuilder.RenameIndex(
                name: "IX_Playlists_UserId",
                table: "Playlist",
                newName: "IX_Playlist_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_LastListenedSongs_UserId",
                table: "LastListenedSong",
                newName: "IX_LastListenedSong_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_LastListenedSongs_DatabaseSongId",
                table: "LastListenedSong",
                newName: "IX_LastListenedSong_DatabaseSongId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Playlist",
                table: "Playlist",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LastListenedSong",
                table: "LastListenedSong",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DatabaseSong",
                table: "DatabaseSong",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DatabaseSongPlaylist_DatabaseSong_SongsId",
                table: "DatabaseSongPlaylist",
                column: "SongsId",
                principalTable: "DatabaseSong",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DatabaseSongPlaylist_Playlist_PlaylistsId",
                table: "DatabaseSongPlaylist",
                column: "PlaylistsId",
                principalTable: "Playlist",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LastListenedSong_DatabaseSong_DatabaseSongId",
                table: "LastListenedSong",
                column: "DatabaseSongId",
                principalTable: "DatabaseSong",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LastListenedSong_Users_UserId",
                table: "LastListenedSong",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Playlist_Users_UserId",
                table: "Playlist",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
