namespace backend_csharp.Models
{
    public class ArtistResponseDto
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? CoverUrl { get; set; }
        public List<string> Genre { get; set; }
    }
}
