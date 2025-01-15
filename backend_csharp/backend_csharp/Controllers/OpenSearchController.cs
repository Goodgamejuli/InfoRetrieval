﻿using backend_csharp.Models;
using backend_csharp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_csharp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpenSearchController : ControllerBase
    {
        /// <summary>
        /// This method creates the index for the opensearchsongs in openSearch.
        /// This should not be called by any api in the frontend but needed to create the index once.
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task <ActionResult> CreateOpenSearchIndexForSongs()
        {
            var response = await OpenSearchService.Instance.CreateIndex();

            return Ok(response);
        }

        [HttpPost("{artistName}")]
        public async Task <ActionResult> IndexSongsOfArtistIntoOpenSearch(string artistName)
        {
            var songs = await SpotifyAPIService.Instance.GetAllTracksOfArtistAsOpenSearchDocument(artistName);

            if (songs == null || songs.Count == 0)
                return BadRequest("No Song was found for the given artist");

            foreach (OpenSearchSongDocument song in songs)
            {
                await OpenSearchService.Instance.IndexNewSong(song);
            }

            return Ok(songs);
        }

        [HttpGet("{songName}")]
        public async Task <ActionResult <OpenSearchSongDocument>> FindSongByName(string songName)
        {
            var song = await OpenSearchService.Instance.SearchForTopSongFittingName(songName);

            return song;
        }

        [HttpGet]
        public async Task<ActionResult <SongDto>> TestGet()
        {
            var track = new SongDto
            {
                Title = "test song", 
                Lyrics = "lorem ipsum oder so",
                Album = "album test",
                Genre = ["Rock", "Pop"],
                Artist = "Eminem",
                SpotifyId = "sdadfsd", 
                Release = "20.24.2001"
            };

            return track;
        }
    }
}
