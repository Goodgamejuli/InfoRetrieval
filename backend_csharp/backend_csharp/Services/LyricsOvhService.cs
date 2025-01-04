﻿namespace backend_csharp.Services
{
    public class LyricsOvhService
    {
        private static LyricsOvhService _instance;
        public static LyricsOvhService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LyricsOvhService();
                }
                return _instance;
            }
        }

        // TODO: Auslagern in appsettings oder so
        private readonly Uri _baseAddress = new Uri("https://api.lyrics.ovh/v1/");

        private HttpClient _client;

        public LyricsOvhService()
        {
            _client = new HttpClient();
            _client.BaseAddress = _baseAddress;
        }

        /// <summary>
        /// Returns the lyric of a song by an artist. Therefore using the lyrics.ovh-Api
        /// </summary>
        /// <param name="artist">Name of the artist</param>
        /// <param name="title">Name of the song</param>
        /// <returns>lyrics of the song, empty string if no lyric was found</returns>
        public async Task <string> GetLyricByArtistAndTitle(string artist, string title)
        {
            // CancelationToken, so that this api call breaks after the duration of 2 seconds 
            // Then there was no lyrics found for the given song, what is possible with the Api we use
            using var cancellationTokenSource = new CancellationTokenSource(200);

            title = OptimizeTitleStringForApiCall(title);

            try
            {
                var response = await _client.GetAsync($"{artist}/{title}", cancellationTokenSource.Token);

                try
                {
                    return await response.Content.ReadAsStringAsync();
                }
                finally
                {
                    response.Dispose();
                }
            }
            catch
            {
                Console.WriteLine("Task Cancelled!");
                return "";
            }
        }

        /// <summary>
        /// The spotify-Api often returns song names as "name - winter Edition" etc.
        /// The OvhApi can't read this songs, so we trim all text after some characters.
        /// Yes this means that, if a song has an "-" in his title, we can't find the lyric
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private string OptimizeTitleStringForApiCall(string title)
        {
            if (title.Contains("-")
                || title.Contains("("))
                title = title.Substring(0, title.IndexOfAny(['-', '(']));

            return title;
        }
    }
}
