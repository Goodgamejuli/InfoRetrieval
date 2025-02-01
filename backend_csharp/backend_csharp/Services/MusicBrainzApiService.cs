using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Browses;
using MetaBrainz.MusicBrainz.Interfaces.Entities;
using MetaBrainz.MusicBrainz.Interfaces.Searches;

namespace backend_csharp.Services;

/// <summary>
///     This class handles all functionality related to the musicBrainz api
/// </summary>
public static class MusicBrainzApiService
{
    #region Public Methods

    /// <summary>
    ///     This method crawls all songs for the specified artist from the musicBrainz api
    /// </summary>
    /// <param name="artistName"> Name of the target artist </param>

    // ReSharper disable once CognitiveComplexity
    public static async Task <List <OpenSearchService.CrawlSongData>?> CrawlAllSongsOfArtist(string artistName)
    {
        Console.WriteLine("Crawling songs from musicBrainz...");

        var query = new Query("InfoManagement", "0.0.1", "tiogiras@gmail.com");

        IArtist? artist = await query.QueryArtist(artistName);

        if (artist == null)
            return null;

        IReadOnlyList <IWork>? tracks = await query.QueryAllTracks(artist);
        IReadOnlyList <IRecording>? recordings = await query.QueryAllRecordings(artist);

        if (tracks == null || recordings == null)
            return null;

        Dictionary <string, PartialDate> recordingDates = new();

        // The api cannot include the relation between the song and the corresponding recordings publication date without
        // an extra api call. To reduce the api calls we save all recordings with their corresponding publication dates for later usage
        foreach (IRecording recording in recordings)
        {
            if (recording.FirstReleaseDate == null)
                continue;

            recordingDates.Add(recording.Id.ToString(), recording.FirstReleaseDate);
        }

        List <OpenSearchService.CrawlSongData> output = [];

        foreach (IWork t in tracks)
        {
            if ((t.Title ?? string.Empty).Contains('(') && (t.Title ?? string.Empty).Contains(')'))
                continue;

            OpenSearchService.CrawlSongData? data = await CreateCrawlSongData(query, artist, t, recordingDates);

            if (data != null)
                output.Add(data);
        }

        return output.Count > 0 ? output : null;
    }

    #endregion

    #region Private Methods

    /// <summary>
    ///     Structure the crawled data for a song
    /// </summary>
    /// <param name="query"> MusicBrainz api query </param>
    /// <param name="artist"> MusicBrainz artist of the song </param>
    /// <param name="track"> MusicBrainz song </param>
    /// <param name="recordingDates"> All recordings of the artist </param>
    private static async Task <OpenSearchService.CrawlSongData?> CreateCrawlSongData(
        Query query,
        IArtist artist,
        IWork track,
        Dictionary <string, PartialDate> recordingDates)
    {
        PartialDate? releaseDate = GetFirstReleaseDateForTrack(track, recordingDates, out Guid releaseRecording);

        string[]? genres = null;

        if (track.Genres is {Count: > 0})
        {
            genres = new string[track.Genres.Count];

            for (var j = 0; j < track.Genres.Count; j++)
            {
                IGenre genre = track.Genres[j];

                if (!string.IsNullOrEmpty(genre.Name))
                    genres[j] = genre.Name;
            }
        }

        var albumTitle = await GetAlbumTitleOfRecording(query, releaseRecording);

        if (string.IsNullOrEmpty(albumTitle))
            return null;

        return new OpenSearchService.CrawlSongData
        {
            id = $"mbid_{track.Id.ToString()}",
            title = track.Title,
            albumId = $"mbid_{releaseRecording.ToString()}",
            albumTitle = albumTitle,
            artistId = $"mbid_{artist.Id.ToString()}",
            artistName = artist.Name,
            genres = genres,
            releaseDate = releaseDate
        };
    }

    /// <summary>
    ///     This method uses the musicBrainz api to get the title of a recording
    /// </summary>
    /// <param name="query"> MusicBrainz api query </param>
    /// <param name="releaseRecording"> ID of the target recording </param>
    /// <returns></returns>
    private static async Task <string?> GetAlbumTitleOfRecording(Query query, Guid releaseRecording)
    {
        try
        {
            IRecording recording = await query.LookupRecordingAsync(releaseRecording, Include.Releases);

            IRelease? release =
                recording.Releases?.FirstOrDefault(release => release.Date == recording.FirstReleaseDate);

            return release?.Title;
        }
        catch (Exception)
        {
            Console.WriteLine("Could not get recording due to invalid mbid returning null");

            return null;
        }
    }

    /// <summary>
    ///     This method searches for the most early release date recorded for the given song
    /// </summary>
    /// <param name="track"> Targeted song </param>
    /// <param name="recordingDates"> Reference to all recordings of the artist </param>
    /// <param name="releaseRecording"> ID of the most early recording </param>
    private static PartialDate? GetFirstReleaseDateForTrack(
        IWork track,
        Dictionary <string, PartialDate> recordingDates,
        out Guid releaseRecording)
    {
        releaseRecording = Guid.Empty;

        if (track.Relationships == null)
            return null;

        Guid[] recordings = track.Relationships.Where(rel => rel.Recording != null).
                                  Select(relationship => relationship.Recording!.Id).
                                  ToArray();

        PartialDate? firstReleaseDate = null;

        foreach (Guid recording in recordings)
        {
            if (!recordingDates.TryGetValue(recording.ToString(), out PartialDate? date))
                continue;

            if (firstReleaseDate != null && firstReleaseDate <= date)
                continue;

            firstReleaseDate = date;
            releaseRecording = recording;
        }

        return firstReleaseDate;
    }

    /// <summary>
    ///     This method uses the musicBrainz api to get all recordings of an artist
    /// </summary>
    /// <param name="query"> MusicBrainz api query </param>
    /// <param name="artist"> Target artist </param>
    private static async Task <IReadOnlyList <IRecording>?> QueryAllRecordings(this Query query, IArtist artist)
    {
        List <IRecording> allRecordings = [];

        var offset = 0;

        IBrowseResults <IRecording> recordings;

        do
        {
            if (offset > 0)
                Console.WriteLine($"...{offset}");

            recordings = await query.BrowseArtistRecordingsAsync(
                artist.Id,
                100,
                offset);

            allRecordings.AddRange(recordings.Results);
            offset += recordings.Results.Count;
        }
        while (recordings.Results.Count > 0);

        Console.WriteLine($"Queried {allRecordings.Count} recordings");

        Console.WriteLine("This could take a while...");

        return allRecordings.Count > 0 ? allRecordings : null;
    }

    /// <summary>
    ///     This method uses the musicBrainz api to get all songs of an artist
    /// </summary>
    /// <param name="query"> MusicBrainz api query </param>
    /// <param name="artist"> Target artist </param>
    private static async Task <IReadOnlyList <IWork>?> QueryAllTracks(this Query query, IArtist artist)
    {
        List <IWork> allTracks = [];

        var offset = 0;

        IBrowseResults <IWork> works;

        do
        {
            if (offset > 0)
                Console.WriteLine($"...{offset}");

            works = await query.BrowseArtistWorksAsync(
                artist.Id,
                100,
                offset,
                Include.RecordingRelationships | Include.Genres);

            allTracks.AddRange(works.Results);
            offset += works.Results.Count;
        }
        while (works.Results.Count > 0);

        Console.WriteLine($"Queried {allTracks.Count} tracks");

        return allTracks.Count > 0 ? allTracks : null;
    }

    /// <summary>
    ///     This method uses the musicBrainz api to find an artist by the name
    /// </summary>
    /// <param name="query"> MusicBrainz api query </param>
    /// <param name="artistName"> Name of the target artist </param>
    private static async Task <IArtist?> QueryArtist(this Query query, string artistName)
    {
        // Queries all artists corresponding to the artist name but only picks the one with the highest score
        ISearchResults <ISearchResult <IArtist>> artists = await query.FindArtistsAsync(artistName, 1, simple: true);

        return artists.TotalResults == 0 ? null : artists.Results[0].Item;
    }

    #endregion
}
