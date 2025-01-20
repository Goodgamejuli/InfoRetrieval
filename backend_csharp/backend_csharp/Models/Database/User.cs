using System.Text.Json.Serialization;

namespace backend_csharp.Models.Database
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        // Relationships
        public ICollection <LastListenedSong> LastListenedSongs { get; set; } = new List<LastListenedSong>();
        public ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
    }
}
