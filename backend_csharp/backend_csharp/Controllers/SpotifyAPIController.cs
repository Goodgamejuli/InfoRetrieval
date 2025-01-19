﻿using backend_csharp.Models;
using backend_csharp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;

namespace backend_csharp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpotifyAPIController : ControllerBase
    {

        [HttpGet("songs/{id}")]
        public async Task <ActionResult <FullTrack>> GetTrackById(string id)
        {
            var track = await SpotifyAPIService.Instance.GetTrackById(id);

            if (track == null)
                return BadRequest($"Song with ID '{id}' wasnt found in the spotifyAPI");

            return Ok(track);
        }

        [HttpGet("songsOfArtist")]
        public async Task <ActionResult<List <OpenSearchSongDocument>>> GetAllPossibleTracksOfAnArtist(string artistName)
        {
            var tracks = await SpotifyAPIService.Instance.GetAllTracksOfArtistAsOpenSearchDocument(artistName);

            if (tracks == null || tracks.Count == 0)
                return BadRequest($"No song was found for Artist {artistName}!");

            return Ok(tracks);
        }

        [HttpGet("{title}/{artist}")]
        public async Task <ActionResult <string>> GetIdOfSong(string title, string artist)
        {
            var id = await SpotifyAPIService.Instance.GetSpotifyIdOfSong(title, artist);
            
            return Ok(string.IsNullOrEmpty(id) ? $"No song with the title {title} of the artist {artist} was found!" : id);
        }
    }
}
