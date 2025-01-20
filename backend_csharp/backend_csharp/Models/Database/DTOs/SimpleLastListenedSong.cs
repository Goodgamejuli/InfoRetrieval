namespace backend_csharp.Models.Database.DTOs
{
    public class SimpleLastListenedSong
    {
        public Guid UserId { get; set; }
        public string DatabaseSongId { get; set; } = null!;
    }
}
