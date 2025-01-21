﻿using backend_csharp.Models;
using backend_csharp.Models.Database;
using backend_csharp.Services;
using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;

namespace backend_csharp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OpenSearchController(DatabaseService databaseService)
    : ControllerBase
{
    #region Public Methods

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
        string query = "title;album;artist;lyrics",
        int hitCount = 10)
    {
        OpenSearchSongDocument[]? osSongs =
            await OpenSearchService.Instance.SearchForTopFittingSongs(query, search, hitCount);

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

    [HttpPost("IndexArtistSongsInOpenSearch_MusicBrainz/{artistName}")]
    public async Task <ActionResult> IndexSongsOfArtistIntoOpenSearchMusicBrainz(string artistName)
    {
        List <OpenSearchSongDocument>? songs =
            await MusicBrainzApiService.GetAllTracksOfArtistAsOpenSearchDocument(artistName);

        if (songs is not {Count: > 0})
            return BadRequest("No song was found for the given artist");

        foreach (OpenSearchSongDocument song in songs)
            await OpenSearchService.Instance.IndexNewSong(song);

        // TODO reenable await databaseService.InsertSongIntoDatabase(song.ToDbSong());
        return Ok(songs);
    }

    [HttpPost("IndexArtistSongsInOpenSearch_Spotify/{artistName}")]
    public async Task <ActionResult> IndexSongsOfArtistIntoOpenSearchSpotify(string artistName)
    {
        Tuple <FullArtist, List <FullAlbum>, List <OpenSearchSongDocument>> data =
            await SpotifyAPIService.Instance.GetAllTracksOfArtistAsOpenSearchDocument(artistName);

        if (data.Item3 is not {Count: > 0})
            return BadRequest("No song was found for the given artist");

        Artist artist = await databaseService.TryInsertOrGetExistingArtist(
            new Artist
            {
                Id = data.Item1.Id,
                Name = data.Item1.Name,
                CoverUrl = data.Item1.Images.Count > 0 ? data.Item1.Images[0].Url : null
            });

        foreach (OpenSearchSongDocument song in data.Item3)
        {
            FullAlbum? fullAlbum = data.Item2.FirstOrDefault(x => x.Name.Equals(song.AlbumTitle));

            Album album = await databaseService.TryInsertOrGetExistingAlbum(
                new Album
                {
                    Id = fullAlbum.Id,
                    Name = fullAlbum.Name,
                    ArtistId = artist.Id,
                    CoverUrl = fullAlbum.Images.Count > 0 ? fullAlbum.Images[0].Url : null
                });

            await OpenSearchService.Instance.IndexNewSong(song);

            await databaseService.InsertSongIntoDatabase(
                new DatabaseSong {Id = song.Id, Embed = song.GenerateSongEmbed(), AlbumId = album.Id});
        }

        return Ok(data.Item3);
    }

    #endregion
}
