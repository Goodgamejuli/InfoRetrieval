using OpenSearch.Client;

namespace backend_csharp.Models
{
/// <summary>
/// This is the data transfer object to send tracks to the frontend
/// </summary>
    public class TrackDto
    {
        public string Title { get; set; }
        public string Lyrics { get; set; }
    }
}
