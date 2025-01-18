using System.Text.Json.Serialization;

namespace backend_csharp.Models.Database
{
    /// <summary>
    /// This model is used to save the last songs a user has listened to.
    /// </summary>
    public class LastListenedSong
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string DatabaseSongId { get; set; } = null!;

        // Relationship
        [JsonIgnore] public User User { get; set;} = null!;
        [JsonIgnore] public DatabaseSong DatabaseSong { get; set; } = null!;
    }
}
