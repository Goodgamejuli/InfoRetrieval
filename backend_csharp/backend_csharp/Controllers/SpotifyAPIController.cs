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

        [HttpGet("track/{id}")]
        public async Task <ActionResult <FullTrack>> GetTrackById(string id)
        {
            var track = await SpotifyAPIService.Instance.GetTrackById(id);

            if (track == null)
                return BadRequest($"Track with ID '{id}' wasnt found in the spotifyAPI");

            return Ok(track);
        }
    }
}
