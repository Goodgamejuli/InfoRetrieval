namespace backend_csharp.Models.Database
{
    /// <summary>
    /// This model defines a table in the database for a song. Here only additional data to each song is saved.
    /// The main properties like (songname, artist, lyrics etc.) are only saved in open-search.
    /// The id of this song should be the same as the id of the corresponding open-search song.
    /// </summary>
    public class DatabaseSong
    {
        public string Id { get; set; }

        // Relationship
        public List <Playlist> Playlists { get; set; } = new List<Playlist>();
        public ICollection <LastListenedSong> LastListenedSongs {get; set;} = new List <LastListenedSong>();
    }
}
