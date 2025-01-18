namespace backend_csharp.Models.Database
{
    /// <summary>
    /// This model defines the playlist table in the database.
    /// Each user can have multiple playlists
    /// </summary>
    public class Playlist
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public Guid UserId { get; set; }

        // Relationships
        public User User { get; set; } = null!;
        public List <DatabaseSong> Songs { get; set; } = new List<DatabaseSong>();
    }
}
