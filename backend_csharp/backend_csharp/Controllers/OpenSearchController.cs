using backend_csharp.Models;
using backend_csharp.Services;
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

        [HttpPost("IndexArtistSongsInOpenSearch_Spotify/{artistName}")]
        public async Task <ActionResult> IndexSongsOfArtistIntoOpenSearchSpotify(string artistName)
        {
            List <OpenSearchSongDocument> songs = await SpotifyAPIService.Instance.GetAllTracksOfArtistAsOpenSearchDocument(artistName);

            if (songs is not {Count: > 0})
                return BadRequest("No song was found for the given artist");

            foreach (OpenSearchSongDocument song in songs)
            {
                await OpenSearchService.Instance.IndexNewSong(song);
            }

            return Ok(songs);
        }
        
        [HttpPost("IndexArtistSongsInOpenSearch_MusicBrainz/{artistName}")]
        public async Task <ActionResult> IndexSongsOfArtistIntoOpenSearchMusicBrainz(string artistName)
        {
            List <OpenSearchSongDocument>? songs = await MusicBrainzApiService.GetAllTracksOfArtistAsOpenSearchDocument(artistName);

            if (songs is not {Count: > 0})
                return BadRequest("No song was found for the given artist");

            foreach (OpenSearchSongDocument song in songs)
            {
                await OpenSearchService.Instance.IndexNewSong(song);
            }

            return Ok(songs);
        }

        [HttpGet("FindSong")]
        public async Task <ActionResult <OpenSearchSongDocument>> FindSong(string query, string search)
        {
            OpenSearchSongDocument? song = await OpenSearchService.Instance.SearchForTopFittingSong(query, search);
            
            if (song == null)
                return BadRequest("No song was found for the given query");

            return Ok(song);
        }
        
        [HttpGet("FindSongsByName/{songName}")]
        public async Task <ActionResult <OpenSearchSongDocument>> FindSongByName(string songName)
        {
            var song = await OpenSearchService.Instance.SearchForTopSongFittingName(songName);

            return song;
        }

        [HttpGet("FindSongsByLyrics/{lyrics}")]
        public async Task<ActionResult<IReadOnlyCollection<OpenSearchSongDocument>>> FindSongsByLyrics(string lyrics)
        {
            var songs = await OpenSearchService.Instance.SearchForSongsByLyrics(lyrics);

            if(songs == null)
                return BadRequest("Kein Song mit der Lyrics gefunden!");

            return Ok(songs);
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
