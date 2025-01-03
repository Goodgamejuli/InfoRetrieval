using backend_csharp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_csharp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpenSearchController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult <TrackDto>> TestGet()
        {
            var track = new TrackDto {Title = "test song", Lyrics = "lorem ipsum oder so",};

            return track;
        }
    }
}
