namespace backend_csharp.Models
{
    public class AlbumResponseDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ArtistName {get; set;}
        public string CoverUrl { get; set; }
        public string ReleaseDate { get; set; }
    }
}
