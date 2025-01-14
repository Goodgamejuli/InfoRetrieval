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
        private const string INDEXNAME = "Song";

        public OpenSearchService()
        {
            var nodeAddress = new Uri("http://localhost:9200");
            var config = new ConnectionSettings(nodeAddress).DefaultIndex("testSong");
            _client = new OpenSearchClient(config);
        }

        /// <summary>
        /// Crate the index for our songDocuments in OpenSearch
        /// </summary>
        /// <returns></returns>
        public async Task CreateIndex()
        {
            await _client.Indices.DeleteAsync(INDEXNAME);

            var response = await _client.Indices.CreateAsync(INDEXNAME,
                                                             x => x.Map<OpenSearchSongDocument>(xx 
                                                                 => xx.AutoMap()));
        }

        /// <summary>
        /// Adds a song to OpenSearch
        /// </summary>
        /// <param name="song"></param>
        /// <returns></returns>
        public async Task IndexNewSong(OpenSearchSongDocument song)
        {
            await _client.IndexAsync(song, i => i.Index(INDEXNAME));
        }

        //public async Task<SearchSongsResponse> Search(SearchSongRequest request, CancellationToken cancellationToken)
        //{
        //    var parameters = request.ToSearchParameter();
        //    var songsResponce = await SearchAsync(parameters, cancellationToken);
        //    var songs = songsResponce.Documents;
        //    var count = songsResponce.Total;
        //    var response = new SearchSongsResponse(songs.Select(x => x.ToSongResponse()), request.PageNumber,
        //                                           request.PageSize, (int)count, (int)Math.Ceiling(count / (double)request.PageSize));

        //    return response;
        //}


    }
}
