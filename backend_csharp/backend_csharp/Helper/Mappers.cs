using backend_csharp.Models;
using backend_csharp.Models.Database;
using backend_csharp.Models.Database.DTOs;

namespace backend_csharp.Helper;

/// <summary>
///     This class stores all mappers between classes
/// </summary>
public static class Mappers
{
    #region Public Methods

    /// <summary>
    ///     This method maps an album to a dto
    /// </summary>
    /// <param name="album"> Target album </param>
    /// <param name="artist"> Name of the artist of the album </param>
    /// <param name="releaseDate"> Release date of the album </param>
    public static AlbumResponseDto ToAlbumResponseDto(this Album album, string artist, string releaseDate)
    {
        return new AlbumResponseDto
        {
            Id = album.Id,
            Name = album.Name ?? string.Empty,
            ArtistName = artist,
            CoverUrl = album.CoverUrl ?? string.Empty,
            ReleaseDate = releaseDate
        };
    }

    /// <summary>
    ///     This method maps an artist to a dto
    /// </summary>
    /// <param name="artist"> Target artist </param>
    /// <param name="genre"> Genre the artist has </param>
    public static ArtistResponseDto ToArtistsResponseDto(this Artist artist, List <string> genre)
    {
        return new ArtistResponseDto {Id = artist.Id, Name = artist.Name, CoverUrl = artist.CoverUrl, Genre = genre};
    }

    /// <summary>
    ///     This method maps a simple song to a last listened song
    /// </summary>
    /// <param name="simpleSong"> Target song </param>
    public static LastListenedSong ToLastListenedSong(this SimpleLastListenedSong simpleSong)
    {
        return new LastListenedSong
        {
            Id = Guid.NewGuid(), UserId = simpleSong.UserId, DatabaseSongId = simpleSong.DatabaseSongId
        };
    }

    /// <summary>
    ///     This method maps a simple user to a user
    /// </summary>
    /// <param name="simpleUser"> Target user </param>
    public static User ToUser(this SimpleUser simpleUser)
    {
        return new User {Id = Guid.NewGuid(), Username = simpleUser.Username, Password = simpleUser.Password};
    }

    #endregion
}
