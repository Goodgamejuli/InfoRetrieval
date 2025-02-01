using backend_csharp.Helper;
using backend_csharp.Models;
using backend_csharp.Models.Database;
using MetaBrainz.MusicBrainz;
using OpenSearch.Client;

namespace backend_csharp.Services;

/// <summary>
///     This class handles all functionality related to the openSearch instance
/// </summary>
public class OpenSearchService
{
    /// <summary>
    ///     Struct like class for temporary storage of the song data
    /// </summary>
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

    /// <summary>
    ///     This method creates a openSearch document by combining spotify and musicBrainz data for one song
    /// </summary>
    /// <param name="spotifySongData"> Target spotify data </param>
    /// <param name="mbSongData"> Target musicBrainz data </param>
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

        // Always prioritise the spotify data over the musicBrainz data due to other apis working with the spotify id

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
                             Number(
                                 d => d 
                                      .Name(n => n.ReleaseDate).
                                      Type(NumberType.Long)
                             ).
                             Keyword(k => k.Name(n => n.Genre))
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

    /// <summary>
    ///     This method finds a song in openSearch by its id
    /// </summary>
    /// <param name="id"> ID of the target song </param>
    public async Task <OpenSearchSongDocument?> FindSongById(string id)
    {
        GetResponse <OpenSearchSongDocument>? response =
            await _client.GetAsync <OpenSearchSongDocument>(id, g => g.Index(IndexName));

        return response.Found ? response.Source : null;
    }

    /// <summary>
    ///     Adds a song to OpenSearch
    /// </summary>
    /// <param name="song"> Song to add to openSearch </param>
    public async Task IndexNewSong(OpenSearchSongDocument? song)
    {
        if (song == null)
            return;

        IndexResponse? response = await _client.IndexAsync(song, i => i.Index(IndexName));

        Console.WriteLine(response.DebugInformation);
    }

    /// <summary>
    ///     This method removes a song from openSearch
    /// </summary>
    /// <param name="id"> ID of the target song </param>
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

    

    /// <summary>
    ///     Search through the openSearch instance to find the most fitting songs, according to all selected values (title, album , artist, lyrics)
    /// </summary>
    /// <param name="query"> Specifies what fields are included when matching the search </param>
    /// <param name="search"> Search criteria (song name) </param>
    /// <param name="genreSearch"> Defines the search criteria if u want to additional search for a genre. Leave it null if not </param>
    /// <param name="dateSearch"> Defines the search criteria if u want to additional search for a date. Leave it null if not</param>
    /// <param name="hitCount"> Number of search hits </param>
    /// <param name="minScoreThreshold"> Minimum score until a entry is accepted as a hit </param>

    // ReSharper disable once CognitiveComplexity
    public async Task <OpenSearchSongDocument[]?> SearchForTopFittingSongs(
        string query,
        string search,
        string? genreSearch,
        string? dateSearch,
        int hitCount,
        float minScoreThreshold)
    {
        if (string.IsNullOrEmpty(search))
            return null;

        search = search.ToLower();

        DateSearch? dateSearchObject = dateSearch?.ConvertStringToDateSearch();

        var queries = query.Split(";");

        if (queries.Length == 0)
            return null;

        var titleBoost = queries.Contains("title") ? 1.5 : -1.0;
        var albumBoost = queries.Contains("album") ? 1.0 : -1.0;
        var artistBoost = queries.Contains("artist") ? 1.0 : -1.0;
        var lyricsBoost = queries.Contains("lyrics") ? 0.1 : -1.0;
        //var genreBoost = queries.Contains("genre") ? 0.25 : -1.0;

        if (titleBoost < 0 && albumBoost < 0 && artistBoost < 0 && lyricsBoost < 0)
            return null;

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
                               })
                                 .Filter(
                                     f =>
                                     {
                                         if (!string.IsNullOrWhiteSpace(genreSearch))
                                         {
                                             f.Terms(t => t
                                                          .Field(ff => ff.Genre)
                                                          .Terms(genreSearch.Split(','))); // Mehrere Genres unterst�tzen
                                         }

                                         if (dateSearchObject != null 
                                             && dateSearchObject.StartDate != null
                                             && dateSearchObject.EndDate != null)
                                         {
                                             f.Range(r => r
                                                          .Field(ff => ff.ReleaseDate)
                                                          .GreaterThanOrEquals(dateSearchObject.StartDate.Value)
                                                          .LessThanOrEquals(dateSearchObject.EndDate.Value));
                                         }

                                         return f; 
                                     }
                                ))).
                   Sort(s => s.Descending(SortSpecialField.Score)));

        if (songs is not {IsValid: true})
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

    /// <summary>
    ///     Search through the openSearch instance to find the most fitting songs, according to all selected values (here only title or lyrics)
    /// </summary>
    /// <param name="query"> Specifies what fields are included when matching the search </param>
    /// <param name="search"> Search criteria (song name) </param>
    /// <param name="genreSearch"> Defines the search criteria if u want to additional search for a genre. Leave it null if not </param>
    /// <param name="dateSearch"> Defines the search criteria if u want to additional search for a date. Leave it null if not</param>
    /// <param name="hitCount"> Number of search hits </param>
    /// <param name="minScoreThreshold"> Minimum score until a entry is accepted as a hit </param>

    // ReSharper disable once CognitiveComplexity
    public async Task<OpenSearchSongDocument[]?> SearchForSongs(
        string query,
        string search,
        string? genreSearch,
        string? dateSearch,
        int hitCount,
        float minScoreThreshold)
    {
        if (string.IsNullOrEmpty(search))
            return null;

        search = search.ToLower();

        DateSearch? dateSearchObject = dateSearch?.ConvertStringToDateSearch();

        var queries = query.Split(";");

        if (queries.Length == 0)
            return null;

        var titleBoost = queries.Contains("title") ? 1.5 : -1.0;
        var lyricsBoost = queries.Contains("lyrics") ? 0.1 : -1.0;

        if (titleBoost < 0 && lyricsBoost < 0)
            return null;

        ISearchResponse<OpenSearchSongDocument>? songs = await _client.SearchAsync<OpenSearchSongDocument>(
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

                                                          if (lyricsBoost > 0)
                                                              f.Field(ff => ff.Lyrics, lyricsBoost);

                                                          return f;
                                                      }).
                                                  Query(search).
                                                  Fuzziness(Fuzziness.Auto).
                                                  MinimumShouldMatch(1));
                                   }

                                   return s;
                               })
                                 .Filter(
                                     f =>
                                     {
                                         if (!string.IsNullOrWhiteSpace(genreSearch))
                                         {
                                             f.Terms(t => t
                                                          .Field(ff => ff.Genre)
                                                          .Terms(genreSearch.Split(','))); // Mehrere Genres unterst�tzen
                                         }

                                         if (dateSearchObject != null
                                             && dateSearchObject.StartDate != null
                                             && dateSearchObject.EndDate != null)
                                         {
                                             f.Range(r => r
                                                          .Field(ff => ff.ReleaseDate)
                                                          .GreaterThanOrEquals(dateSearchObject.StartDate.Value)
                                                          .LessThanOrEquals(dateSearchObject.EndDate.Value));
                                         }

                                         return f;
                                     }
                                ))).
                   Sort(s => s.Descending(SortSpecialField.Score)));

        if (songs is not { IsValid: true })
            return null;

        List<OpenSearchSongDocument> filteredSongs = [];

        IHit<OpenSearchSongDocument>[] hits = songs.Hits.ToArray();
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

    /// <summary>
    ///     Combine spotify and musicBrainz data to extract a album title
    /// </summary>
    /// <param name="spotifySongData"> Target spotify data </param>
    /// <param name="mbSongData"> Target musicBrainz data </param>
    private static string GenerateOsdAlbumTitle(CrawlSongData? spotifySongData, CrawlSongData? mbSongData)
    {
        if (spotifySongData != null && !string.IsNullOrEmpty(spotifySongData.albumTitle))
            return spotifySongData.albumTitle;

        if (mbSongData != null && !string.IsNullOrEmpty(mbSongData.albumTitle))
            return mbSongData.albumTitle;

        return "";
    }

    /// <summary>
    ///     Combine spotify and musicBrainz data to extract an artist name
    /// </summary>
    /// <param name="spotifySongData"> Target spotify data </param>
    /// <param name="mbSongData"> Target musicBrainz data </param>
    private static string GenerateOsdArtistName(CrawlSongData? spotifySongData, CrawlSongData? mbSongData)
    {
        if (spotifySongData != null && !string.IsNullOrEmpty(spotifySongData.artistName))
            return spotifySongData.artistName;

        if (mbSongData != null && !string.IsNullOrEmpty(mbSongData.artistName))
            return mbSongData.artistName;

        return "";
    }

    /// <summary>
    ///     Combine spotify and musicBrainz data to extract genre
    /// </summary>
    /// <param name="spotifySongData"> Target spotify data </param>
    /// <param name="mbSongData"> Target musicBrainz data </param>
    private static List <string> GenerateOsdGenre(CrawlSongData? spotifySongData, CrawlSongData? mbSongData)
    {
        List <string> genres = [];

        if (spotifySongData?.genres is {Length: > 0})
            genres.AddRange(spotifySongData.genres.Select(genre => genre.ToLower()));

        if (mbSongData?.genres is {Length: > 0})
            genres.AddRange(mbSongData.genres.Select(genre => genre.ToLower()));

        return genres.Distinct().ToList();
    }

    /// <summary>
    ///     Combine spotify and musicBrainz data to extract an id
    /// </summary>
    /// <param name="spotifySongData"> Target spotify data </param>
    /// <param name="mbSongData"> Target musicBrainz data </param>
    private static string? GenerateOsdId(CrawlSongData? spotifySongData, CrawlSongData? mbSongData)
    {
        if (spotifySongData != null && !string.IsNullOrEmpty(spotifySongData.id))
            return spotifySongData.id;

        if (mbSongData != null && !string.IsNullOrEmpty(mbSongData.id))
            return mbSongData.id;

        return null;
    }

    /// <summary>
    ///     This method calls the lyrics service to generate lyrics based on the artist and the song
    /// </summary>
    /// <param name="artist"> Artist of the target song </param>
    /// <param name="title"> Target song (name) </param>
    private static async Task <string> GenerateOsdLyrics(string artist, string title)
    {
        if (string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(title))
            return "";

        return await LyricsOvhService.Instance.GetLyricByArtistAndTitle(artist, title);
    }

    /// <summary>
    ///     Combine spotify and musicBrainz data to extract a release date
    /// </summary>
    /// <param name="spotifySongData"> Target spotify data </param>
    /// <param name="mbSongData"> Target musicBrainz data </param>
    private static long GenerateOsdReleaseDate(CrawlSongData? spotifySongData, CrawlSongData? mbSongData)
    {
        var validSpotify = spotifySongData is {releaseDate: not null};
        var validMb = mbSongData is {releaseDate: not null};

        switch (validSpotify)
        {
            case true when validMb:
            {
                PartialDate spotifyDate = spotifySongData!.releaseDate!;
                PartialDate mbDate = mbSongData!.releaseDate!;

                return spotifyDate < mbDate ? spotifyDate.ToUnixTimestamp() : mbDate.ToUnixTimestamp();
            }

            case true:
                return spotifySongData!.releaseDate!.ToUnixTimestamp();
        }

        return validMb ? mbSongData!.releaseDate!.ToUnixTimestamp() : 0;
    }

    /// <summary>
    ///     Combine spotify and musicBrainz data to extract a title
    /// </summary>
    /// <param name="spotifySongData"> Target spotify data </param>
    /// <param name="mbSongData"> Target musicBrainz data </param>
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

    /// <summary>
    ///     Search through the openSearch instance to find the most fitting albums
    /// </summary>
    /// <param name="search"> Search criteria (album name) </param>
    /// <param name="maxHitCount"> Number of search hits </param>
    /// <param name="dbService"> Reference to the database service </param>

    // ReSharper disable once CognitiveComplexity
    public async Task <List <AlbumResponseDto>?> SearchForAlbum(
        string search,
        int maxHitCount,
        DatabaseService dbService)
    {
        search = search.ToLower();

        // Finding all OpenSearchSongDocuments, where Album is fitting the search
        ISearchResponse <OpenSearchSongDocument>? openSearchResponse =
            await _client.SearchAsync <OpenSearchSongDocument>(
                x => x.Index(IndexName).
                       Size(maxHitCount).
                       Query(
                           q => q.Bool(
                               b => b.Should(
                                   s =>
                                   {
                                       if (search.Contains('*') || search.Contains('?'))
                                       {
                                           s.Wildcard(
                                               w => w.Field(ff => ff.AlbumTitle).
                                                      Value(search));
                                       }
                                       else
                                       {
                                           s.Match(
                                               m => m.Field(f => f.AlbumTitle).Query(search).Fuzziness(Fuzziness.Auto));
                                       }

                                       return s;
                                   }))).
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
                album.ToAlbumResponseDto(songResponse.ArtistName, songResponse.ReleaseDate.ToDateOnly().ToString()));
        }

        return albumSortContainer.Values.ToList();
    }

    /// <summary>
    ///     Search through the openSearch instance to find the most fitting song in a specific album
    /// </summary>
    /// <param name="albumTitle"> Search criteria (album name) </param>
    /// <param name="search"> Search criteria (song name) </param>
    /// <param name="minScoreThreshold"> Minimum score needed until an entry is considered a hit </param>
    /// <param name="dbService"> Reference to the database service </param>

    // ReSharper disable once CognitiveComplexity
    public async Task <List <SongDto>?> FindMatchingSongsInAlbum(
        string albumTitle,
        DatabaseService dbService,
        string? search,
        float minScoreThreshold)
    {
        if (string.IsNullOrEmpty(albumTitle) )
            return null;

        search = search?.ToLower();

        ISearchResponse <OpenSearchSongDocument> openSearchResponse =
            await _client.SearchAsync <OpenSearchSongDocument>(
                s => s.Index(IndexName).
                      Size(100).
                       Query(
                           q => q.Bool(
                               b => b.Filter( // Filter --> value must be fitting
                                          f => f.Term(
                                              t => t.Field(ff => ff.AlbumTitle.Suffix("keyword")).Value(albumTitle)
                                          )
                                      ).
                                      Must(
                                          m =>
                                          {
                                              // Early return if u want to find all songs of album
                                              if (string.IsNullOrEmpty(search))
                                              {
                                                  return null;
                                              }

                                              if (search.Contains('*') || search.Contains('?'))
                                              {
                                                  m.Wildcard(
                                                      w => w.Field(f => f.Title).
                                                             Value(search));
                                              }
                                              else
                                              {
                                                  m.Match(
                                                      mm => mm.Field(f => f.Title).
                                                               Query(search).
                                                               Fuzziness(Fuzziness.Auto));
                                              }

                                              return m;
                                          }
                                      ))));

        if (openSearchResponse is not {IsValid: true})
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

    #region Artist Search

    /// <summary>
    ///     Search through the openSearch instance to find the most fitting artist
    /// </summary>
    /// <param name="search"> Search criteria (artist name) </param>
    /// <param name="maxHitCount"> Number of search hits </param>
    /// <param name="dbService"> Reference to the database service </param>

    // ReSharper disable once CognitiveComplexity
    public async Task<List<ArtistResponseDto>?> SearchForArtist(
        string search,
        int maxHitCount,
        DatabaseService dbService)
    {
        search = search.ToLower();

        // Finding all OpenSearchSongDocuments, where Artists is fitting the search
        ISearchResponse<OpenSearchSongDocument>? openSearchResponse =
            await _client.SearchAsync<OpenSearchSongDocument>(
                x => x.Index(IndexName).
                       Size(maxHitCount).
                       Query(
                           q => q.Bool(
                               b => b.Should(
                                   s =>
                                   {
                                       if (search.Contains('*') || search.Contains('?'))
                                       {
                                           s.Wildcard(
                                               w => w.Field(ff => ff.ArtistName).
                                                      Value(search));
                                       }
                                       else
                                       {
                                           s.Match(
                                               m => m.Field(f => f.ArtistName).Query(search).Fuzziness(Fuzziness.Auto));
                                       }

                                       return s;
                                   }))).
                       Sort(s => s.Descending(SortSpecialField.Score)));

        if (!openSearchResponse.IsValid)
            return null;

        // Reduce multiple found artists to one
        Dictionary<string, ArtistResponseDto> artistSortContainer = new();

        foreach (OpenSearchSongDocument? songResponse in openSearchResponse.Documents)
        {
            if (artistSortContainer.ContainsKey(songResponse.ArtistName))
                continue;

            Artist? artist = await dbService.GetArtistBySong(songResponse.Id);

            if (artist == null)
                continue;

            artistSortContainer.Add(artist.Name!, artist.ToArtistsResponseDto(songResponse.Genre));
        }

        return artistSortContainer.Values.ToList();
    }

    public async Task<List<SongDto>?> FindMatchingSongsOfArtist(
        string artist,
        DatabaseService dbService,
        string? search,
        float minScoreThreshold)
    {
        if (string.IsNullOrEmpty(artist))
            return null;

        search = search?.ToLower();

        ISearchResponse<OpenSearchSongDocument> openSearchResponse =
            await _client.SearchAsync<OpenSearchSongDocument>(
                s => s.Index(IndexName)
                      .Size(100)
                      .Query(
                           q => q.Bool(
                               b => b.Filter( // Filter --> value must be fitting
                                          f => f.Term(
                                              t => t.Field(ff => ff.ArtistName.Suffix("keyword")).Value(artist)
                                          )
                                      ).
                                      Must(
                                          m =>
                                          {
                                              // Early return if u want to find all songs of album
                                              if (string.IsNullOrEmpty(search))
                                              {
                                                  return null;
                                              }

                                              if (search.Contains('*') || search.Contains('?'))
                                              {
                                                  m.Wildcard(
                                                      w => w.Field(f => f.Title).
                                                             Value(search));
                                              }
                                              else
                                              {
                                                  m.Match(
                                                      mm => mm.Field(f => f.Title).
                                                               Query(search).
                                                               Fuzziness(Fuzziness.Auto));
                                              }

                                              return m;
                                          }
                                      ))));

        if (openSearchResponse is not { IsValid: true })
            return null;

        List<SongDto> filteredSongs = new();

        IHit<OpenSearchSongDocument>[] hits = openSearchResponse.Hits.ToArray();
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

    #region Date Search

    public async Task <List <SongDto>?> FindSongsAccordingToDate(string date,  DatabaseService dbService, int hitCount)
    {
        if(string.IsNullOrEmpty(date)) return null;

        var dateSearch = date.ConvertStringToDateSearch();

        if(dateSearch == null) return null;

        var openSearchResponse = await _client.SearchAsync<OpenSearchSongDocument>(s => s
                                                                       .Index(IndexName)
                                                                       .Size(hitCount)
                                                                       .Query(q => q
                                                                                  .Range(r => r
                                                                                             .Field(f => f.ReleaseDate) 
                                                                                             .GreaterThanOrEquals(dateSearch.StartDate)
                                                                                             .LessThanOrEquals(dateSearch.EndDate)
                                                                                  )
                                                                       )
                    );

        if (openSearchResponse is not { IsValid: true })
            return null;

        List<SongDto> filteredSongs = new();

        IHit<OpenSearchSongDocument>[] hits = openSearchResponse.Hits.ToArray();
        OpenSearchSongDocument[] documents = openSearchResponse.Documents.ToArray();

        for (var i = 0; i < openSearchResponse.Documents.Count; i++)
        {
            DatabaseSong? dbSong = await dbService.GetSong(documents[i].Id);

            if (dbSong == null)
                continue;

            filteredSongs.Add(new SongDto(documents[i], dbSong));
        }

        return filteredSongs;
    }

    #endregion

    #region Date Search

    public async Task<List<SongDto>?> FindSongsAccordingToGenre(string genreSearch, DatabaseService dbService, int hitCount)
    {
        if (string.IsNullOrEmpty(genreSearch)) return null;

        var openSearchResponse = await _client.SearchAsync<OpenSearchSongDocument>(s => s
            .Index(IndexName)
            .Size(hitCount)
            .Query(q => q
                    .Terms(t => t
                     .Field(ff => ff.Genre)
                     .Terms(genreSearch.Split(',')))
            )
        );  

        if (openSearchResponse is not { IsValid: true })
            return null;

        List<SongDto> filteredSongs = new();

        IHit<OpenSearchSongDocument>[] hits = openSearchResponse.Hits.ToArray();
        OpenSearchSongDocument[] documents = openSearchResponse.Documents.ToArray();

        for (var i = 0; i < openSearchResponse.Documents.Count; i++)
        {
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
