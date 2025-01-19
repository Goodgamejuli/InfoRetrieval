using backend_csharp.Models;
using backend_csharp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;

namespace backend_csharp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MusicBrainzApiController : ControllerBase
    {
        [HttpGet("tracksOfArtist")]
        public async Task <ActionResult<List <OpenSearchSongDocument>>> GetAllPossibleTracksOfAnArtist(string artistName)
        {
            var tracks = await MusicBrainzApiService.GetAllTracksOfArtistAsOpenSearchDocument(artistName);

            if (tracks == null || tracks.Count == 0)
                return BadRequest($"No track was found for Artist {artistName}!");

            return Ok(tracks);
        }
    }
}
