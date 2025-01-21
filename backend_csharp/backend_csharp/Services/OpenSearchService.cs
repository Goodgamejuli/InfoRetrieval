using backend_csharp.Models;
using OpenSearch.Client;

namespace backend_csharp.Services;

public class OpenSearchService
{
    private const string Indexname = "songs_index";

    private readonly OpenSearchClient _client;

    private OpenSearchService()
    {
        var nodeAddress = new Uri("http://localhost:9200");
        ConnectionSettings? config = new ConnectionSettings(nodeAddress).DefaultIndex("songs");
        _client = new OpenSearchClient(config);
    }

    #region Public Methods

    /// <summary>
    ///     Crate the index for our songDocuments in OpenSearch
    /// </summary>
    /// <returns></returns>
    public async Task <string> CreateIndex()
    {
        await _client.Indices.DeleteAsync(Indexname);

        try
        {
            CreateIndexResponse? response = await _client.Indices.CreateAsync(
                Indexname,
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

    /// <summary>
    ///     Adds a song to OpenSearch
    /// </summary>
    /// <param name="song"></param>
    /// <returns></returns>
    public async Task IndexNewSong(OpenSearchSongDocument song)
    {
        IndexResponse? response = await _client.IndexAsync(song, i => i.Index(Indexname));

        Console.WriteLine(response.DebugInformation);
    }

    // ReSharper disable once CognitiveComplexity
    public async Task <OpenSearchSongDocument[]?> SearchForTopFittingSongs(string query, string search, int hitCount)
    {
        var queries = query.Split(";");

        var titleBoost = queries.Contains("title") ? 1.5 : -1.0;
        var albumBoost = queries.Contains("album") ? 1.0 : -1.0;
        var artistBoost = queries.Contains("artist") ? 1.0 : -1.0;
        var lyricsBoost = queries.Contains("lyrics") ? 0.25 : -1.0;

        ISearchResponse <OpenSearchSongDocument>? songs = await _client.SearchAsync <OpenSearchSongDocument>(
            x => x.Index(Indexname).Size(hitCount).
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
                           ))));

        if (songs == null || songs.Documents.Count == 0)
            return null;
        
        Console.WriteLine($"Found {songs.Documents!.Count} song(s)");

        List <OpenSearchSongDocument> output = songs.Documents.ToList();

        output.Sort();
        
        return output?.ToArray();
    }

    public async Task <OpenSearchSongDocument?> FindSongById(string id)
    {
        GetResponse <OpenSearchSongDocument>? response =
            await _client.GetAsync <OpenSearchSongDocument>(id, g => g.Index(Indexname));

        return response.Found ? response.Source : null;
    }

    #endregion

    #region Singleton

    private static OpenSearchService? s_instance;

    public static OpenSearchService Instance => s_instance ??= new OpenSearchService();

    #endregion
}
