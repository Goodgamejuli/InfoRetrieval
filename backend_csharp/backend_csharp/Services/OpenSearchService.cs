using backend_csharp.Helper;
using backend_csharp.Models;
using backend_csharp.Models.Database;
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
            // Creating an Index for the song document
            CreateIndexResponse? response = await _client.Indices.CreateAsync(
                IndexName,
                c => c.Map <OpenSearchSongDocument>(
                    m => m.Properties(
                        p => p

                             // Indexing title as Text but a keyword-search is also possible
                             .Text(
                                 t => t.Name(n => n.Title).Fields(f => f.Keyword(k => k.Name("keyword")))
                             ).
                             Text(
                                 t => t.Name(n => n.AlbumTitle).Fields(f => f.Keyword(k => k.Name("keyword")))
                             ).
                             Text(
                                 t => t.Name(n => n.ArtistName).Fields(f => f.Keyword(k => k.Name("keyword")))
                             ).
                             Text(t => t.Name(n => n.Lyrics)).
                             Date(
                                 d => d // Feld als Datum definieren
                                      .Name(n => n.ReleaseDate).
                                      Format("yyyy-MM-dd") // Optional: Datumsformat
                             ).
                             Keyword(k => k.Name(n => n.Genre)) // Genre bleibt ein Keyword
                    )
                )
            );

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

    public async Task RemoveSong(string id)
    {
        DeleteResponse? deleteResponse =
            await _client.DeleteAsync <OpenSearchSongDocument>(id, d => d.Index(IndexName));

        Console.WriteLine(
            deleteResponse.IsValid
                ? $"Document with the id [{id}] was removed from open search!"
                : $"Document with the id [{id}] could not be removed from open search: {
                    deleteResponse.ServerError?.Error?.Reason}!");
    }

    #region Artist Search

    public async Task <List <ArtistResponseDto>?> SearchForArtist(
        string search,
        int maxHitCount,
        DatabaseService dbService)
    {
        // Finding all OpenSearchSongDocuments, where Artists is fitting the search
        ISearchResponse <OpenSearchSongDocument>? openSearchResponse =
            await _client.SearchAsync <OpenSearchSongDocument>(
                x => x.Index(IndexName).
                       Size(maxHitCount).
                       Query(q => q.Match(m => m.Field(f => f.ArtistName).Query(search).Fuzziness(Fuzziness.Auto))).
                       Sort(s => s.Descending(SortSpecialField.Score)));

        if (!openSearchResponse.IsValid)
            return null;

        // Reduce multiple found artists to one
        Dictionary <string, ArtistResponseDto> artistSortContainer = new();

        foreach (OpenSearchSongDocument? songResponse in openSearchResponse.Documents)
        {
            if (artistSortContainer.ContainsKey(songResponse.ArtistName))
                continue;

            Artist? artist = await dbService.GetArtistBySong(songResponse.Id);

            if (artist == null)
                continue;

            artistSortContainer.Add(artist.Name, artist.ToArtistsResponseDto(songResponse.Genre));
        }

        return artistSortContainer.Values.ToList();
    }

    #endregion

    // ReSharper disable once CognitiveComplexity
    public async Task <OpenSearchSongDocument[]?> SearchForTopFittingSongs(
        string query,
        string search,
        int hitCount,
        float minScoreThreshold)
    {
        if (string.IsNullOrEmpty(search))
            return null;

        search = search.ToLower();

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
                               s =>
                               {
                                   if (search.Contains('*') || search.Contains('?'))
                                   {
                                       if (titleBoost > 0)
                                       {
                                           s.Wildcard(
                                               w => w.Field(ff => ff.Title).Value(search).Boost(titleBoost));
                                       }

                                       if (albumBoost > 0)
                                       {
                                           s.Wildcard(
                                               w => w.Field(ff => ff.AlbumTitle).
                                                      Value(search).
                                                      Boost(albumBoost));
                                       }

                                       if (artistBoost > 0)
                                       {
                                           s.Wildcard(
                                               w => w.Field(ff => ff.ArtistName).
                                                      Value(search).
                                                      Boost(artistBoost));
                                       }

                                       if (lyricsBoost > 0)
                                       {
                                           s.Wildcard(
                                               w => w.Field(ff => ff.Lyrics).
                                                      Value(search).
                                                      Boost(lyricsBoost));
                                       }
                                   }
                                   else
                                   {
                                       s.MultiMatch(
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
                                                  Fuzziness(Fuzziness.Auto));
                                   }

                                   return s;
                               }
                           ))).
                   Sort(s => s.Descending(SortSpecialField.Score)));

        if (songs == null || !songs.IsValid)
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

    #region Album Search

    public async Task <List <AlbumResponseDto>?> SearchForAlbum(
        string search,
        int maxHitCount,
        DatabaseService dbService)
    {
        // Finding all OpenSearchSongDocuments, where Album is fitting the search
        ISearchResponse <OpenSearchSongDocument>? openSearchResponse =
            await _client.SearchAsync <OpenSearchSongDocument>(
                x => x.Index(IndexName).
                       Size(maxHitCount).
                       Query(q => q.Match(m => m.Field(f => f.AlbumTitle).Query(search).Fuzziness(Fuzziness.Auto))).
                       Sort(s => s.Descending(SortSpecialField.Score)));

        if (!openSearchResponse.IsValid)
            return null;

        // Reduce multiple found albums to one
        Dictionary <string, AlbumResponseDto> albumSortContainer = new();

        foreach (OpenSearchSongDocument? songResponse in openSearchResponse.Documents)
        {
            if (albumSortContainer.ContainsKey(songResponse.AlbumTitle))
                continue;

            Album? album = await dbService.GetAlbumBySong(songResponse.Id);

            if (album == null || string.IsNullOrEmpty(album.Name))
                continue;

            albumSortContainer.Add(
                album.Name,
                album.ToAlbumResponseDto(songResponse.ArtistName, songResponse.ReleaseDate));
        }

        return albumSortContainer.Values.ToList();
    }

    public async Task <List <SongDto>> FindMatchingSongsInAlbum(
        string albumTitle,
        DatabaseService dbService,
        float minScoreThreshold = 1,
        string? search = null)
    {
        ISearchResponse <OpenSearchSongDocument>? openSearchResponse =
            await _client.SearchAsync <OpenSearchSongDocument>(
                s => s.Index(IndexName).
                       Query(
                           q => q.Bool(
                               b => b.Filter( // Filter --> value must be fitting
                                          f => f.Term(
                                              t => t.Field(ff => ff.AlbumTitle.Suffix("keyword")).Value(albumTitle)
                                          )
                                      ).
                                      Must(
                                          m => search != null
                                              ? m.MultiMatch(
                                                  mm => mm.Fields(
                                                               f => f.Field(ff => ff.Title)
                                                           ).
                                                           Query(search).
                                                           Fuzziness(Fuzziness.Auto)
                                              )
                                              : null
                                      ))));

        if (openSearchResponse == null || !openSearchResponse.IsValid)
            return null;

        List <SongDto> filteredSongs = new();

        IHit <OpenSearchSongDocument>[] hits = openSearchResponse.Hits.ToArray();
        OpenSearchSongDocument[] documents = openSearchResponse.Documents.ToArray();

        for (var i = 0; i < openSearchResponse.Documents.Count; i++)
        {
            // Return if threshold wasn't hit
            if (hits[i].Score < minScoreThreshold)
                continue;

            DatabaseSong? dbSong = await dbService.GetSong(documents[i].Id);

            if (dbSong == null)
                continue;

            filteredSongs.Add(new SongDto(documents[i], dbSong));
        }

        return filteredSongs;
    }

    #endregion

    #region Singleton

    private static OpenSearchService? s_instance;

    public static OpenSearchService Instance => s_instance ??= new OpenSearchService();

    #endregion
}
