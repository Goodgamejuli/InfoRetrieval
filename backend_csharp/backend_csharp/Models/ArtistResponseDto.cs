namespace backend_csharp.Models;

/// <summary>
///     This class is a data transfer object for a requested artist
/// </summary>
public class ArtistResponseDto
{
    public string Id {get; set;}

    public string? Name {get; set;}

    public string? CoverUrl {get; set;}

    public List <string> Genre {get; set;}
}
