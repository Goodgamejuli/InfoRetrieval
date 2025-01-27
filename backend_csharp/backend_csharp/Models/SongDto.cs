using backend_csharp.Models.Database;

namespace backend_csharp.Models
{
/// <summary>
/// This is the data transfer object to send tracks to the frontend
/// </summary>
    public class SongDto
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Lyrics { get; set; }

        public string Album { get; set; }

        public string Release { get; set; }

        public string Artist { get; set; }

        public List<string> Genre { get; set; }
        
        public string? Embed { get; set; }
        
        public string? Cover { get; set; }

        public SongDto(OpenSearchSongDocument song, DatabaseSong dbEntry)
        {
            Id = song.Id;
            Title = song.Title;
            Album = song.AlbumTitle;
            Artist = song.ArtistName;
            Lyrics = song.Lyrics;
            Genre = song.Genre;
            Release = song.ReleaseDate;
            Embed = dbEntry.Embed;
            Cover = dbEntry.Album.CoverUrl;
        }
    }
}
