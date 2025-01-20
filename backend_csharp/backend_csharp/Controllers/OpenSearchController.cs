using backend_csharp.Models;
using backend_csharp.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend_csharp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OpenSearchController : ControllerBase
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

    [HttpGet("FindSong")]
    public async Task <ActionResult <OpenSearchSongDocument>> FindSongs(
        string search,
        string query = "title;album;artist;lyrics",
        int hitCount = 10)
    {
        OpenSearchSongDocument[]? songs =
            await OpenSearchService.Instance.SearchForTopFittingSongs(query, search, hitCount);

        if (songs == null)
            return BadRequest("No song was found for the given query");

        return Ok(songs);
    }

    /*[HttpGet("FindSong")]
    public async Task<ActionResult<OpenSearchSongDocument>> FindSongs([FromBody] SearchQuery searchParameters)
    {
        OpenSearchSongDocument[]? songs =
            await OpenSearchService.Instance.SearchForTopFittingSongs(searchParameters);

        if (songs == null)
            return BadRequest("No song was found for the given query");

        return Ok(songs); 
    }*/

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
            await OpenSearchService.Instance.IndexNewSong(song);

        return Ok(songs);
    }

    [HttpPost("IndexArtistSongsInOpenSearch_Spotify/{artistName}")]
    public async Task <ActionResult> IndexSongsOfArtistIntoOpenSearchSpotify(string artistName)
    {
        List <OpenSearchSongDocument> songs =
            await SpotifyAPIService.Instance.GetAllTracksOfArtistAsOpenSearchDocument(artistName);

        if (songs is not {Count: > 0})
            return BadRequest("No song was found for the given artist");

        foreach (OpenSearchSongDocument song in songs)
            await OpenSearchService.Instance.IndexNewSong(song);

        return Ok(songs);
    }

    #endregion
}
