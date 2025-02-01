using System.Text.Json.Serialization;

namespace backend_csharp.Models.Database;

/// <summary>
///     This class holds all data for any album
/// </summary>
public class Album
{
    public string Id {get; set;}

    public string ArtistId {get; set;}

    public string? Name {get; set;}

    public string? CoverUrl {get; set;}

    // Relationships

    [JsonIgnore] public Artist Artist {get; set;}

    [JsonIgnore] public ICollection <DatabaseSong> Songs {get; set;} = new List <DatabaseSong>();
}
