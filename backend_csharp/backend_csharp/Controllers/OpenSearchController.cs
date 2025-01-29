using backend_csharp.Models;
using backend_csharp.Models.Database;
using backend_csharp.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend_csharp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OpenSearchController(DatabaseService databaseService)
    : ControllerBase
{
    #region Public Methods

    [HttpPost("CrawlAllSongsOfArtist")]
    public async Task <ActionResult <SongDto[]?>> CrawlAllSongsOfArtist(
        string artistName,
        bool useSpotifyApi = true,
        bool useMusicBrainzApi = true)
    {
        OpenSearchService.CrawlSongData[]? spotifyData = null;
        List <OpenSearchService.CrawlSongData>? musicBrainzData = null;

        Console.WriteLine($"Crawling songs for artist [{artistName}]...");

        if (useSpotifyApi)
            spotifyData = await SpotifyApiService.Instance.CrawlAllSongsOfArtist(artistName);

        if (useMusicBrainzApi)
            musicBrainzData = await MusicBrainzApiService.CrawlAllSongsOfArtist(artistName);

        if (spotifyData is not {Length: > 0} && musicBrainzData is not {Count: > 0})
            return BadRequest("At least one api is required to crawl songs of an artist!");

        int totalCount;
        var currentCount = 0;

        if (spotifyData != null && musicBrainzData != null)
        {
            totalCount = spotifyData.Length +
                         musicBrainzData.
                             Count(
                                 mb => !spotifyData.Any(
                                     x => x.title.Equals(mb.title) && x.artistName.Equals(mb.artistName)));
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
                    x => x.title.Equals(spotify.title) && x.artistName.Equals(spotify.artistName));

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

    [HttpGet("FindSongs")]
    public async Task <ActionResult <SongDto[]>> FindSongs(
        string search,
        string query = "title;album;artist;lyrics;genre",
        int hitCount = 10,
        float minScoreThreshold = 0.5f)
    {
        OpenSearchSongDocument[]? osSongs =
            await OpenSearchService.Instance.SearchForTopFittingSongs(query, search, hitCount, minScoreThreshold);

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

    [HttpGet("FindArtists")]
    public async Task <ActionResult <List <ArtistResponseDto>>> FindArtists(string search, int maxHitCount)
    {
        var artists = await OpenSearchService.Instance.SearchForArtist(search, maxHitCount, databaseService);

        if (artists == null)
            return BadRequest("Error while searching for Artist!");

        return Ok(artists);
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

    [HttpGet("FindAlbums")]
    public async Task<ActionResult<List<AlbumResponseDto>>> FindAlbums(string search, int maxHitCount)
    {
        List <AlbumResponseDto>? artists = await OpenSearchService.Instance.SearchForAlbum(search, maxHitCount, databaseService);

        if (artists == null)
            return BadRequest("Error while searching for Album!");

        return Ok(artists);
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

    #endregion

    #region Private Methods

    private async Task <SongDto?> GenerateAndAddSongToOpenSearch(
        OpenSearchService.CrawlSongData? spotifyData,
        OpenSearchService.CrawlSongData? musicBrainzData)
    {
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

        Artist artist = await databaseService.TryInsertOrGetExistingArtist(
            new Artist {Id = artistId, Name = osDocument.ArtistName, CoverUrl = data.Item3});

        Album album = await databaseService.TryInsertOrGetExistingAlbum(
            new Album {Id = albumId, Name = osDocument.AlbumTitle, ArtistId = artist.Id, CoverUrl = data.Item5});

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

        if (songData.Item2 != null)
            await OpenSearchService.Instance.RemoveSong(songData.Item2);

        Console.WriteLine("Song was added to openSearch!");

        return new SongDto(osDocument, songData.Item1);
    }

    #endregion
}
