namespace backend_csharp.Models;

/// <summary>
///     This class is a data transfer object for a requested album
/// </summary>
public class AlbumResponseDto
{
    public string Id {get; set;}

    public string Name {get; set;}

    public string ArtistName {get; set;}

    public string CoverUrl {get; set;}

    public string ReleaseDate {get; set;}
}
