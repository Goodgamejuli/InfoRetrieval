using backend_csharp.Models;
using OpenSearch.Client;

namespace backend_csharp.Services
{

public class OpenSearchService
{
    #region Singleton

    private static OpenSearchService _instance;

    public static OpenSearchService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new OpenSearchService();
            }

            return _instance;
        }
    }

    #endregion

    private OpenSearchClient _client;
    private const string INDEXNAME = "songs_index";

    public OpenSearchService()
    {
        var nodeAddress = new Uri("http://localhost:9200");
        var config = new ConnectionSettings(nodeAddress).DefaultIndex("songs");
        _client = new OpenSearchClient(config);
    }

    /// <summary>
    /// Crate the index for our songDocuments in OpenSearch
    /// </summary>
    /// <returns></returns>
    public async Task<string> CreateIndex()
    {
        await _client.Indices.DeleteAsync(INDEXNAME);

        try
        {
            var response = await _client.Indices.CreateAsync(
                INDEXNAME,
                x => x.Map <OpenSearchSongDocument>(
                    xx
                        => xx.AutoMap()));

            return response.DebugInformation;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            throw e;
        }
    }

    /// <summary>
    /// Adds a song to OpenSearch
    /// </summary>
    /// <param name="song"></param>
    /// <returns></returns>
    public async Task IndexNewSong(OpenSearchSongDocument song)
    {
        var response = await _client.IndexAsync(song, i => i.Index(INDEXNAME));

        Console.WriteLine(response.DebugInformation);
    }

    public async Task <OpenSearchSongDocument> SearchForTopSongFittingName(string songName)
    {
        var song = await _client.SearchAsync <OpenSearchSongDocument>(
            x => x.Index(INDEXNAME).
                   Query(q => q.
                             Match(m => m.Field(field => field.Title).Query(songName))));

        if (song.IsValid)
            return song.Documents.First();

        return null;
    }
}

}
