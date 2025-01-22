using backend_csharp.Models;
using MetaBrainz.MusicBrainz;
using OpenSearch.Client;

namespace backend_csharp.Services;

public class OpenSearchService
{
    public class CrawlSongData
    {
        public string? albumCoverUrl;
        public string? albumId;
        public string? albumTitle;
        public string? artistCoverUrl;
        public string? artistId;
        public string? artistName;
        public string[]? genres;
        public string? id;
        public PartialDate? releaseDate;
        public string? title;
    }

    private const string IndexName = "songs_index";

    private readonly OpenSearchClient _client;

    private OpenSearchService()
    {
        var nodeAddress = new Uri("http://localhost:9200");
        ConnectionSettings? config = new ConnectionSettings(nodeAddress).DefaultIndex("songs");
        _client = new OpenSearchClient(config);
    }

    #region Public Methods

    public static async Task <Tuple <OpenSearchSongDocument?, string?, string?, string?, string?>?>
        GenerateOpenSearchDocument(
            CrawlSongData? spotifySongData,
            CrawlSongData? mbSongData)
    {
        if (spotifySongData == null && mbSongData == null)
        {
            Console.WriteLine("Failed: No data provided!");
            
            return null;
        }

        var id = GenerateOsdId(spotifySongData, mbSongData);

        if (string.IsNullOrEmpty(id))
        {
            Console.WriteLine("Failed: Missing song id!");
            
            return null;
        }

        var title = GenerateOsdTitle(spotifySongData, mbSongData);
        var artistName = GenerateOsdArtistName(spotifySongData, mbSongData);

        var osDocument = new OpenSearchSongDocument
        {
            Id = id,
            Title = title,
            AlbumTitle = GenerateOsdAlbumTitle(spotifySongData, mbSongData),
            ArtistName = artistName,
            ReleaseDate = GenerateOsdReleaseDate(spotifySongData, mbSongData),
            Genre = GenerateOsdGenre(spotifySongData, mbSongData),
            Lyrics = await GenerateOsdLyrics(artistName, title)
        };

        return new Tuple <OpenSearchSongDocument?, string?, string?, string?, string?>(
            osDocument,
            spotifySongData?.artistId ?? (mbSongData?.artistId ?? string.Empty),
            spotifySongData?.artistCoverUrl,
            spotifySongData?.albumId ?? (mbSongData?.albumId ?? string.Empty),
            spotifySongData?.albumCoverUrl);
    }

    /// <summary>
    ///     Crate the index for our songDocuments in OpenSearch
    /// </summary>
    /// <returns></returns>
    public async Task <string> CreateIndex()
    {
        await _client.Indices.DeleteAsync(IndexName);

        try
        {
            CreateIndexResponse? response = await _client.Indices.CreateAsync(
                IndexName,
                x => x.Map <OpenSearchSongDocument>(
                    xx
                        => xx.AutoMap()));

            return response.DebugInformation;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            throw;
        }
    }

    public async Task <OpenSearchSongDocument?> FindSongById(string id)
    {
        GetResponse <OpenSearchSongDocument>? response =
            await _client.GetAsync <OpenSearchSongDocument>(id, g => g.Index(IndexName));

        return response.Found ? response.Source : null;
    }

    /// <summary>
    ///     Adds a song to OpenSearch
    /// </summary>
    /// <param name="song"></param>
    public async Task IndexNewSong(OpenSearchSongDocument? song)
    {
        if (song == null)
            return;

        IndexResponse? response = await _client.IndexAsync(song, i => i.Index(IndexName));

        Console.WriteLine(response.DebugInformation);
    }

    // ReSharper disable once CognitiveComplexity
    public async Task <OpenSearchSongDocument[]?> SearchForTopFittingSongs(string query, string search, int hitCount, float minScoreThreshold)
    {
        if (string.IsNullOrEmpty(search))
            return null;

        var queries = query.Split(";");

        var titleBoost = queries.Contains("title") ? 1.5 : -1.0;
        var albumBoost = queries.Contains("album") ? 1.0 : -1.0;
        var artistBoost = queries.Contains("artist") ? 1.0 : -1.0;
        var lyricsBoost = queries.Contains("lyrics") ? 0.25 : -1.0;

        ISearchResponse <OpenSearchSongDocument>? songs = await _client.SearchAsync <OpenSearchSongDocument>(
            x => x.Index(IndexName).
                   Size(hitCount).
                   Query(
                       q => q.Bool(
                           b => b.Should(
                               s => s.MultiMatch(
                                   m => m.Fields(
                                              f =>
                                              {
                                                  if (titleBoost > 0)
                                                      f.Field(ff => ff.Title, titleBoost);

                                                  if (albumBoost > 0)
                                                      f.Field(ff => ff.AlbumTitle, albumBoost);

                                                  if (artistBoost > 0)
                                                      f.Field(ff => ff.ArtistName, artistBoost);

                                                  if (lyricsBoost > 0)
                                                      f.Field(ff => ff.Lyrics, lyricsBoost);

                                                  return f;
                                              }).
                                          Query(search).
                                          Fuzziness(Fuzziness.Auto))
                           ))).Sort(s => s.Descending(SortSpecialField.Score)));

        if (songs == null)
            return null;

        List <OpenSearchSongDocument> filteredSongs = [];

        IHit <OpenSearchSongDocument>[] hits = songs.Hits.ToArray();
        OpenSearchSongDocument[] documents = songs.Documents.ToArray();
        
        for (var i = 0; i < songs.Documents.Count; i++)
        {
            if (hits[i].Score < minScoreThreshold)
                continue;
            
            filteredSongs.Add(documents[i]);
        }
        
        Console.WriteLine($"Found {filteredSongs.Count} song(s)");

        return filteredSongs.ToArray();
    }

    #endregion

    #region Private Methods

    private static string GenerateOsdAlbumTitle(CrawlSongData? spotifySongData, CrawlSongData? mbSongData)
    {
        if (spotifySongData != null && !string.IsNullOrEmpty(spotifySongData.albumTitle))
            return spotifySongData.albumTitle;

        if (mbSongData != null && !string.IsNullOrEmpty(mbSongData.albumTitle))
            return mbSongData.albumTitle;

        return "";
    }

    private static string GenerateOsdArtistName(CrawlSongData? spotifySongData, CrawlSongData? mbSongData)
    {
        if (spotifySongData != null && !string.IsNullOrEmpty(spotifySongData.artistName))
            return spotifySongData.artistName;

        if (mbSongData != null && !string.IsNullOrEmpty(mbSongData.artistName))
            return mbSongData.artistName;

        return "";
    }

    private static List <string> GenerateOsdGenre(CrawlSongData? spotifySongData, CrawlSongData? mbSongData)
    {
        List <string> genres = [];

        if (spotifySongData?.genres is {Length: > 0})
            genres.AddRange(spotifySongData.genres.Select(genre => genre.ToLower()));

        if (mbSongData?.genres is {Length: > 0})
            genres.AddRange(mbSongData.genres.Select(genre => genre.ToLower()));

        return genres.Distinct().ToList();
    }

    private static string? GenerateOsdId(CrawlSongData? spotifySongData, CrawlSongData? mbSongData)
    {
        if (spotifySongData != null && !string.IsNullOrEmpty(spotifySongData.id))
            return spotifySongData.id;

        if (mbSongData != null && !string.IsNullOrEmpty(mbSongData.id))
            return mbSongData.id;

        return null;
    }

    private static async Task <string> GenerateOsdLyrics(string artist, string title)
    {
        if (string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(title))
            return "";

        return await LyricsOvhService.Instance.GetLyricByArtistAndTitle(artist, title);
    }

    private static string GenerateOsdReleaseDate(CrawlSongData? spotifySongData, CrawlSongData? mbSongData)
    {
        var validSpotify = spotifySongData is {releaseDate: not null};
        var validMb = mbSongData is {releaseDate: not null};

        if (validSpotify && validMb)
        {
            PartialDate spotifyDate = spotifySongData!.releaseDate!;
            PartialDate mbDate = mbSongData!.releaseDate!;

            return spotifyDate < mbDate ? spotifyDate.ToString() : mbDate.ToString();
        }

        if (validSpotify)
            return spotifySongData!.releaseDate!.ToString();

        if (validMb)
            return mbSongData!.releaseDate!.ToString();

        return "";
    }

    private static string GenerateOsdTitle(CrawlSongData? spotifySongData, CrawlSongData? mbSongData)
    {
        if (spotifySongData != null && !string.IsNullOrEmpty(spotifySongData.title))
            return spotifySongData.title;

        if (mbSongData != null && !string.IsNullOrEmpty(mbSongData.title))
            return mbSongData.title;

        return "";
    }

    #endregion

    #region Singleton

    private static OpenSearchService? s_instance;

    public static OpenSearchService Instance => s_instance ??= new OpenSearchService();

    #endregion

    public async Task RemoveSong(string id)
    {
        DeleteResponse? deleteResponse =
            await _client.DeleteAsync <OpenSearchSongDocument>(id, d => d.Index(IndexName));

        Console.WriteLine(
            deleteResponse.IsValid
                ? $"Document with the id [{id}] was removed from open search!"
                : $"Document with the id [{id}] could not be removed from open search: {deleteResponse.ServerError?.Error?.Reason}!");
    }
}
