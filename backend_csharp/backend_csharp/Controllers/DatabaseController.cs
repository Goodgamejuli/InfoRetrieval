using backend_csharp.Models.Database;
using backend_csharp.Models.Database.DTOs;
using backend_csharp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_csharp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<WeatherForecastController> _logger;

        public DatabaseController(ILogger<WeatherForecastController> logger, DatabaseService service)
        {
            _logger = logger;
            _databaseService = service;
        }

        #region User Specific

        [HttpPost("User")]
        public async Task <ActionResult> AddNewUser([FromBody] SimpleUserDto user)
        {
            if (await _databaseService.InsertUserToDatabase(user))
                return Ok($"User added!");
            else
                return BadRequest("User could not get added to database!");
        }

        #endregion

        #region Database Song Specific

        [HttpPost("DatabaseSong")]
        public async Task <ActionResult> AddDatabaseSong([FromBody] DatabaseSong song)
        {
            if (await _databaseService.InsertSongIntoDatabase(song))
                return Ok("Song Added");
            else
                return BadRequest("Song could not get added");
        }

        [HttpGet("DatabaseSong/{id}")]
        public async Task <ActionResult <DatabaseSong>> GetDatabaseSong(string id)
        {
            DatabaseSong song = await _databaseService.GetSongFromDatabase(id);
            
            if (song == null)
                return BadRequest($"No song was found with the id {id}");

            return Ok(song);
        }

        #endregion
    }
}
