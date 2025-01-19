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
    }
}
