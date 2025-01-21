using backend_csharp.Models;
using backend_csharp.Models.Database;
using backend_csharp.Services;
using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;

namespace backend_csharp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OpenSearchController (DatabaseService databaseService)
    : ControllerBase
{
    #region Public Methods

    /// <summary>
    ///     This method creates the index for the opensearchsongs in openSearch.
    ///     This should not be called by any api in the frontend but needed to create the index once.
    /// </summary>
    /// <returns></returns>
    [HttpPut]
    public async Task <ActionResult> CreateOpenSearchIndexForSongs()
    {
        var response = await OpenSearchService.Instance.CreateIndex();

        return Ok(response);
    }

    [HttpGet("FindSongs")]
    public async Task <ActionResult <SongDto[]>> FindSongs(
        string search,
        string query = "title;album;artist;lyrics",
        int hitCount = 10)
    {
        OpenSearchSongDocument[]? songs =
            await OpenSearchService.Instance.SearchForTopFittingSongs(query, search, hitCount);

        if (songs == null)
            return BadRequest("No song was found for the given query");

        SongDto[] output = new SongDto[songs.Length];

        for (var i = 0; i < songs.Length; i++)
        {
            OpenSearchSongDocument song = songs[i];

            DatabaseSong dbEntry = await databaseService.GetSong(song.Id);

            output[i] = new SongDto(song, dbEntry);
        }

        return Ok(output);
    }

    [HttpGet("FindSong/{id}")]
    public async Task <ActionResult <OpenSearchSongDocument>> FindSongById(string id)
    {
        var song = await OpenSearchService.Instance.FindSongById(id);

        if (song == null)
            return BadRequest($"No song was found for the given id {id}");

        return Ok(song);
    }

    [HttpPost("IndexArtistSongsInOpenSearch_MusicBrainz/{artistName}")]
    public async Task <ActionResult> IndexSongsOfArtistIntoOpenSearchMusicBrainz(string artistName)
    {
        List <OpenSearchSongDocument>? songs =
            await MusicBrainzApiService.GetAllTracksOfArtistAsOpenSearchDocument(artistName);

        if (songs is not {Count: > 0})
            return BadRequest("No song was found for the given artist");
        
        foreach (OpenSearchSongDocument song in songs)
        {
            await OpenSearchService.Instance.IndexNewSong(song);
            // TODO reenable await databaseService.InsertSongIntoDatabase(song.ToDbSong());
        }
        
        return Ok(songs);
    }

    [HttpPost("IndexArtistSongsInOpenSearch_Spotify/{artistName}")]
    public async Task <ActionResult> IndexSongsOfArtistIntoOpenSearchSpotify(string artistName)
    {
        Tuple <FullArtist, List <FullAlbum>, List <OpenSearchSongDocument>> data =
            await SpotifyAPIService.Instance.GetAllTracksOfArtistAsOpenSearchDocument(artistName);
        
        if (data.Item3 is not {Count: > 0})
            return BadRequest("No song was found for the given artist");
        
        Artist artist = await databaseService.TryInsertOrGetExistingArtist(new Artist
        {
            Id = data.Item1.Id,
            Name = data.Item1.Name,
            CoverUrl = data.Item1.Images.Count > 0 ? data.Item1.Images[0].Url : null
        });
        
        foreach (OpenSearchSongDocument song in data.Item3)
        {
            FullAlbum? fullAlbum = data.Item2.FirstOrDefault(x => x.Name.Equals(song.AlbumTitle));
            
            Album album = await databaseService.TryInsertOrGetExistingAlbum(new Album
            {
                Id = fullAlbum.Id,
                Name = fullAlbum.Name,
                ArtistId = artist.Id,
                CoverUrl = fullAlbum.Images.Count > 0 ? fullAlbum.Images[0].Url : null
            });
            
            await OpenSearchService.Instance.IndexNewSong(song);
            await databaseService.InsertSongIntoDatabase(new DatabaseSong
            {
                Id = song.Id,
                Embed = song.GenerateSongEmbed(),
                AlbumId = album.Id
            });
        }
        
        return Ok(data.Item3);
    }

    #endregion
}
