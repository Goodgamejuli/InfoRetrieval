namespace backend_csharp.Services;

/// <summary>
///     This class handles all functionality related to the lyrics api
/// </summary>
public class LyricsOvhService
{
    private static LyricsOvhService? s_instance;

    public static LyricsOvhService Instance => s_instance ??= new LyricsOvhService();

    private readonly Uri _baseAddress = new("https://api.lyrics.ovh/v1/");

    private readonly HttpClient _client;

    private LyricsOvhService()
    {
        _client = new HttpClient();
        _client.BaseAddress = _baseAddress;
    }

    #region Public Methods

    /// <summary>
    ///     Returns the lyric of a song by an artist. Therefor using the lyrics.ovh-Api
    /// </summary>
    /// <param name="artist"> Name of the artist </param>
    /// <param name="title"> Name of the song </param>
    /// <returns> Lyrics of the song, empty string if no lyric was found </returns>
    public async Task <string> GetLyricByArtistAndTitle(string artist, string title)
    {
        // Cancelation Token, so that this api call breaks after the duration of 2 seconds 
        // Then there were no lyrics found for the given song, what is possible with the Api we use
        using var cancellationTokenSource = new CancellationTokenSource(2000);

        title = OptimizeTitleStringForApiCall(title);

        try
        {
            HttpResponseMessage response = await _client.GetAsync($"{artist}/{title}", cancellationTokenSource.Token);

            try
            {
                return await response.Content.ReadAsStringAsync(cancellationTokenSource.Token);
            }
            finally
            {
                response.Dispose();
            }
        }
        catch
        {
            Console.WriteLine("Task cancelled due to lyrics issue!");

            return "";
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    ///     The spotify-Api often returns song names as "name - winter Edition" etc.
    ///     The OvhApi can't read these songs, so we trim all text after some characters.
    ///     Yes this means that, if a song has an "-" in his title, we can't find the lyric
    /// </summary>
    /// <param name="title"></param>
    private static string OptimizeTitleStringForApiCall(string title)
    {
        if (title.Contains('-') || title.Contains('('))
            title = title[..title.IndexOfAny(['-', '('])];

        return title;
    }

    #endregion
}
