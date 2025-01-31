using backend_csharp.Models;
using backend_csharp.Models.Database;
using backend_csharp.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend_csharp.Controllers;

/// <summary>
///     This api controller handles all api calls corresponding with the openSearch instance
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class OpenSearchController(DatabaseService databaseService)
    : ControllerBase
{
    #region Public Methods

    /// <summary>
    ///     This method crawls and saves all songs of the specified artist
    ///     The data can be crawled from spotify or musicBrainz
    /// </summary>
    /// <param name="artistName"> Name of the target artist </param>
    /// <param name="useSpotifyApi"> If true the spotify api is used </param>
    /// <param name="useMusicBrainzApi"> If true the musicBrainz api is used </param>
    [HttpPost("CrawlAllSongsOfArtist")]

    // ReSharper disable once CognitiveComplexity
    public async Task <ActionResult <SongDto[]?>> CrawlAllSongsOfArtist(
        string artistName,
        bool useSpotifyApi = true,
        bool useMusicBrainzApi = true)
    {
        OpenSearchService.CrawlSongData[]? spotifyData = null;
        List <OpenSearchService.CrawlSongData>? musicBrainzData = null;

        Console.WriteLine($"Crawling songs for artist [{artistName}]...");

        if (!useSpotifyApi && !useMusicBrainzApi)
            return BadRequest("At least one api is required to crawl songs of an artist!");

        if (useSpotifyApi)
            spotifyData = await SpotifyApiService.Instance.CrawlAllSongsOfArtist(artistName);

        if (useMusicBrainzApi)
            musicBrainzData = await MusicBrainzApiService.CrawlAllSongsOfArtist(artistName);

        if (spotifyData is not {Length: > 0} && musicBrainzData is not {Count: > 0})
            return BadRequest("No song was found on either api!");

        // These values are simply used to log the progress into the console
        int totalCount;
        var currentCount = 0;

        // Calculate the total count of found songs based on the results of both api queries
        if (spotifyData != null && musicBrainzData != null)
        {
            totalCount = spotifyData.Length +
                         musicBrainzData.
                             Count(
                                 mb => !spotifyData.Any(
                                     x => x.title!.Equals(mb.title) && x.artistName!.Equals(mb.artistName)));
        }
        else
            totalCount = spotifyData?.Length ?? 0 + musicBrainzData?.Count ?? 0;

        List <SongDto> output = [];

        if (spotifyData is {Length: > 0})
        {
            // We iterate over all found spotify elements and see if we can find a corresponding music brainz element
            // If we find a music brainz element we merge their data
            // If we don't find a music brainz we simply use the spotify data
            // If we did found and merged music brainz data we remove the merged data from the list
            foreach (OpenSearchService.CrawlSongData spotify in spotifyData)
            {
                currentCount++;

                Console.WriteLine($"Trying to generate os Document for song {currentCount}/{totalCount}");

                OpenSearchService.CrawlSongData? musicBrainz = musicBrainzData?.FirstOrDefault(
                    x => x.title!.Equals(spotify.title) && x.artistName!.Equals(spotify.artistName));

                if (musicBrainz != null)
                    musicBrainzData!.Remove(musicBrainz);

                SongDto? songDto = await GenerateAndAddSongToOpenSearch(spotify, musicBrainz);

                if (songDto != null)
                    output.Add(songDto);
            }
        }

        if (musicBrainzData is not {Count: > 0})
            return output.ToArray();

        // If there are still songs remaining that did not get removed (where not found by spotify) therefor only exist
        // on music brainz we can simply iterate over all found elements and add them with no spotify data to merge with
        foreach (OpenSearchService.CrawlSongData musicBrainz in musicBrainzData)
        {
            currentCount++;

            Console.WriteLine($"Trying to generate os Document for song {currentCount}/{totalCount}");

            SongDto? songDto = await GenerateAndAddSongToOpenSearch(null, musicBrainz);

            if (songDto != null)
                output.Add(songDto);
        }

        return output.ToArray();
    }

    /// <summary>
    ///     This method creates the index for the openSearch songs in openSearch.
    ///     This should not be called by any api in the frontend but needed to create the index once.
    /// </summary>
    [HttpPut]
    public async Task <ActionResult> CreateOpenSearchIndexForSongs()
    {
        var response = await OpenSearchService.Instance.CreateIndex();

        return Ok(response);
    }

    /// <summary>
    ///     This method returns a number of corresponding albums
    /// </summary>
    /// <param name="search"> Search criteria for the target albums </param>
    /// <param name="maxHitCount"> Number of max returned elements </param>
    [HttpGet("FindAlbums")]
    public async Task <ActionResult <List <AlbumResponseDto>>> FindAlbums(string search, int maxHitCount)
    {
        List <AlbumResponseDto>? artists =
            await OpenSearchService.Instance.SearchForAlbum(search, maxHitCount, databaseService);

        if (artists == null)
            return BadRequest("Error while searching for Album!");

        return Ok(artists);
    }

    /// <summary>
    ///     This method returns a number of corresponding artists
    /// </summary>
    /// <param name="search"> Search criteria for the target artists </param>
    /// <param name="maxHitCount"> Number of max returned elements </param>
    [HttpGet("FindArtists")]
    public async Task <ActionResult <List <ArtistResponseDto>>> FindArtists(string search, int maxHitCount)
    {
        List <ArtistResponseDto>? artists =
            await OpenSearchService.Instance.SearchForArtist(search, maxHitCount, databaseService);

        if (artists == null)
            return BadRequest("Error while searching for Artist!");

        return Ok(artists);
    }

    /// <summary>
    ///     This method returns the corresponding song from the openSearch instance as a dto
    /// </summary>
    /// <param name="id"> ID of the target song </param>
    [HttpGet("FindSong")]
    public async Task <ActionResult <SongDto>> FindSongById(string id)
    {
        OpenSearchSongDocument? osSong = await OpenSearchService.Instance.FindSongById(id);

        if (osSong == null)
            return BadRequest($"No song was found for the given id [{id}]!");

        DatabaseSong? dbSong = await databaseService.GetSong(id);

        if (dbSong == null)
            return BadRequest($"The found os song with the id [{id}] has not been added to the database!");

        return Ok(new SongDto(osSong, dbSong));
    }

    /// <summary>
    ///     This method returns all songs from the openSearch instance as a dto that are the most fitting for the criteria
    /// </summary>
    /// <param name="search"> Search criteria for the target artists </param>
    /// <param name="query"> Defines what aspects should be used to search the most fitting songs </param>
    /// <param name="genreSearch"> Defines the search criteria if u want to additional search for a genre. Leave it null if not </param>
    /// <param name="dateSearch"> Defines the search criteria if u want to additional search for a date. Leave it null if not</param>
    /// <param name="hitCount"> Number of max returned elements </param>
    /// <param name="minScoreThreshold"> Minimum value the score needs to have for the element to be marked as a hit </param>
    [HttpGet("FindTopSongs")]
    public async Task <ActionResult <SongDto[]>> FindTopFittingSongs(
        string search,
        string query = "title;album;artist;lyrics",
        string? genreSearch = null,
        string? dateSearch = null,
        int hitCount = 10,
        float minScoreThreshold = 0.5f)
    {
        OpenSearchSongDocument[]? osSongs =
            await OpenSearchService.Instance.SearchForTopFittingSongs(query, search,  genreSearch, dateSearch, hitCount, minScoreThreshold);

        if (osSongs == null)
            return BadRequest("No song was found for the given query");

        SongDto[] output = new SongDto[osSongs.Length];

        for (var i = 0; i < osSongs.Length; i++)
        {
            OpenSearchSongDocument osSong = osSongs[i];

            DatabaseSong? dbSong = await databaseService.GetSong(osSong.Id);

            if (dbSong == null)
                return BadRequest($"The found os song with the id [{osSong.Id}] has not been added to the database!");

            output[i] = new SongDto(osSong, dbSong);
        }

        return Ok(output);
    }

    /// <summary>
    ///     This method returns all songs from the openSearch instance as a dto that are the most fitting for the criteria
    /// </summary>
    /// <param name="search"> Search criteria for the target artists </param>
    /// <param name="query"> Defines what aspects should be used to search the most fitting songs </param>
    /// <param name="genreSearch"> Defines the search criteria if u want to additional search for a genre. Leave it null if not </param>
    /// <param name="dateSearch"> Defines the search criteria if u want to additional search for a date. Leave it null if not</param>
    /// <param name="hitCount"> Number of max returned elements </param>
    /// <param name="minScoreThreshold"> Minimum value the score needs to have for the element to be marked as a hit </param>
    [HttpGet("FindSongs")]
    public async Task<ActionResult<SongDto[]>> FindSongs(
        string search,
        string query = "title;lyrics",
        string? genreSearch = null,
        string? dateSearch = null,
        int hitCount = 10,
        float minScoreThreshold = 0.5f)
    {
        OpenSearchSongDocument[]? osSongs =
            await OpenSearchService.Instance.SearchForSongs(query, search, genreSearch, dateSearch, hitCount, minScoreThreshold);

        if (osSongs == null)
            return BadRequest("No song was found for the given query");

        SongDto[] output = new SongDto[osSongs.Length];

        for (var i = 0; i < osSongs.Length; i++)
        {
            OpenSearchSongDocument osSong = osSongs[i];

            DatabaseSong? dbSong = await databaseService.GetSong(osSong.Id);

            if (dbSong == null)
                return BadRequest($"The found os song with the id [{osSong.Id}] has not been added to the database!");

            output[i] = new SongDto(osSong, dbSong);
        }

        return Ok(output);
    }

    [HttpGet("FindSongsOfArtist")]
    public async Task <ActionResult <List <SongDto>>> FindSongsOfArtist(
        string artist,
        string? search,
        float minScoreThreshold)
    {
        List<SongDto>? songs = await OpenSearchService.Instance.FindMatchingSongsOfArtist(
            artist,
            databaseService,
            search,
            minScoreThreshold);

        if (songs == null)
            return BadRequest("Error while searching for songs of artist!");

        return Ok(songs);
    }

    [HttpGet("FindSongsInAlbum")]
    public async Task <ActionResult <List <SongDto>>> FindSongsInAlbum(string albumTitle, string? search, float minScoreThreshold)
    {
        List <SongDto>? songs = await OpenSearchService.Instance.FindMatchingSongsInAlbum(
            albumTitle,
            databaseService,
            search,
            minScoreThreshold);

        if (songs == null)
            return BadRequest("Error while searching for songs in album!");

        return Ok(songs);
    }

    [HttpGet("FindSongsByDate")]
    public async Task <ActionResult <List <SongDto>>> FindSongsByDate(string dateSearch)
    {
        List <SongDto>? songs = await OpenSearchService.Instance.FindSongsAccordingToDate(dateSearch, databaseService);

        if (songs == null)
            return BadRequest("Error while searching for date of songs");

        return Ok(songs);
    }

    #endregion

    #region Private Methods

    /// <summary>
    ///     This method generates and adds a song to the openSearch instance
    /// </summary>
    /// <param name="spotifyData"> Data provided by the spotify api </param>
    /// <param name="musicBrainzData"> Data provided by the musicBrainz api </param>
    private async Task <SongDto?> GenerateAndAddSongToOpenSearch(
        OpenSearchService.CrawlSongData? spotifyData,
        OpenSearchService.CrawlSongData? musicBrainzData)
    {
        // Create an openSearch document by merging the two api data together
        Tuple <OpenSearchSongDocument?, string?, string?, string?, string?>? data =
            await OpenSearchService.GenerateOpenSearchDocument(spotifyData, musicBrainzData);

        OpenSearchSongDocument? osDocument = data?.Item1;

        if (osDocument == null)
            return null;

        var artistId = data!.Item2;

        if (string.IsNullOrEmpty(artistId))
            return null;

        var albumId = data.Item4;

        if (string.IsNullOrEmpty(albumId))
            return null;

        // If the artist of the song has not been added to the database add it
        // If the artist has been added but only has musicBrainz data update it with spotify data
        Artist artist = await databaseService.TryInsertOrGetExistingArtist(
            new Artist {Id = artistId, Name = osDocument.ArtistName, CoverUrl = data.Item3});

        // If the album of the song has not been added to the database add it
        // If the album has been added but only has musicBrainz data update it with spotify data
        Album album = await databaseService.TryInsertOrGetExistingAlbum(
            new Album {Id = albumId, Name = osDocument.AlbumTitle, ArtistId = artist.Id, CoverUrl = data.Item5});

        // If the song has not been added to the database add it
        // If the song has been added but only has musicBrainz data update it with spotify data
        Tuple <DatabaseSong, string?> songData = await databaseService.TryInsertOrGetExistingSong(
            new DatabaseSong
            {
                Id = osDocument.Id,
                Title = osDocument.Title,
                Embed = osDocument.GenerateSongEmbed(),
                AlbumId = album.Id
            });

        Console.WriteLine("Song was added to the database!");

        await OpenSearchService.Instance.IndexNewSong(osDocument);

        // If the song did have musicBrainz only data and was updated with spotify data remove the old entry on the openSearch instance
        if (songData.Item2 != null)
            await OpenSearchService.Instance.RemoveSong(songData.Item2);

        Console.WriteLine("Song was added to openSearch!");

        return new SongDto(osDocument, songData.Item1);
    }

    #endregion
}
