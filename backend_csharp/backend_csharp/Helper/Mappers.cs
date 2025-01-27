using backend_csharp.Models;
using backend_csharp.Models.Database;
using backend_csharp.Models.Database.DTOs;

namespace backend_csharp.Helper
{
    public static class Mappers
    {
        public static User ToUser(this SimpleUser simpleUser)
        {
            return new User()
            {
                Id = Guid.NewGuid(),
                Username = simpleUser.Username,
                Password = simpleUser.Password
            };
        }

        public static LastListenedSong ToLastListenedSong(this SimpleLastListenedSong simpleSong)
        {
            return new LastListenedSong()
            {
                Id = Guid.NewGuid(), UserId = simpleSong.UserId, DatabaseSongId = simpleSong.DatabaseSongId
            };
        }

        public static ArtistResponseDto ToArtistsResponseDto(this Artist artist, List <string> genre)
        {
            return new ArtistResponseDto
            {
                Id = artist.Id, Name = artist.Name, CoverUrl = artist.CoverUrl, Genre = genre
            };
        }

        public static AlbumResponseDto ToAlbumResponseDto(this Album album, string artist, string releaseDate)
        {
            return new AlbumResponseDto
            {
                Id = album.Id, Name = album.Name, ArtistName = artist, CoverUrl = album.CoverUrl, ReleaseDate = releaseDate
            };
        }
    }
}
