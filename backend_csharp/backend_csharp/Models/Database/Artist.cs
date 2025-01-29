using System.Text.Json.Serialization;

namespace backend_csharp.Models.Database;

/// <summary>
///     This class holds all data for any artist
/// </summary>
public class Artist
{
    public string Id {get; set;}

    public string? Name {get; set;}

    public string? CoverUrl {get; set;}

    // Relationships

    [JsonIgnore] public ICollection <Album> Albums {get; set;} = new List <Album>();
}
