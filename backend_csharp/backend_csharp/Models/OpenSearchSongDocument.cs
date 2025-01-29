using OpenSearch.Client;

namespace backend_csharp.Models;

/// <summary>
///     This class defines the structure of a document of a track, that shall get saved in open search.
/// </summary>
public class OpenSearchSongDocument
{
    [Text]
    public string Id {get; set;}

    [Text]
    public string Title {get; set;}

    [Text]
    public string Lyrics {get; set;}

    [Text]
    public string AlbumTitle {get; set;}

    [Date]
    public string ReleaseDate {get; set;}

    [Text]
    public string ArtistName {get; set;}

    [Keyword]
    public List <string> Genre {get; set;}

    #region Public Methods

    public string? GenerateSongEmbed()
    {
        return Id.StartsWith("mbid_") ? null : $"https://open.spotify.com/embed/track/{Id}?utm_source=generator";
    }

    #endregion
}
