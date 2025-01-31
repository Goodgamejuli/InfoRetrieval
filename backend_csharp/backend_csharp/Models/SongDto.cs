using backend_csharp.Helper;
using backend_csharp.Models.Database;

namespace backend_csharp.Models;

/// <summary>
///     This is the data transfer object to send tracks to the frontend
/// </summary>
public class SongDto(OpenSearchSongDocument song, DatabaseSong dbEntry)
{
    public string Id {get; set;} = song.Id;

    public string Title {get; set;} = song.Title;

    public string Lyrics {get; set;} = song.Lyrics;

    public string Album {get; set;} = song.AlbumTitle;

    public string Release {get; set;} = song.ReleaseDate.ToDateOnly().ToString();

    public string Artist {get; set;} = song.ArtistName;

    public List <string> Genre {get; set;} = song.Genre;

    public string? Embed {get; set;} = dbEntry.Embed;

    public string? Cover {get; set;} = dbEntry.Album.CoverUrl;
}
