using backend_csharp.Models;
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
        private readonly ILogger<DatabaseController> _logger;

        public DatabaseController(ILogger<DatabaseController> logger, DatabaseService service)
        {
            _logger = logger;
            _databaseService = service;
        }

        /// <summary>
        /// Deletes the old db and creates a new.
        /// All data will get deleted
        /// </summary>
        /// <returns></returns>
        [HttpPost("RemigrateDatabase")]
        public ActionResult RemigrateDatabase()
        {
            var creationSuccessful = _databaseService.RemigrateDatabase();

            if (!creationSuccessful)
                return BadRequest("Database could not get instantiated again");

            return Ok("New database successfully created!");
        }

        #region User Specific

        [HttpPost("Users")]
        public async Task <ActionResult> AddNewUser([FromBody] SimpleUser user)
        {
            if (await _databaseService.InsertUserToDatabase(user))
                return Ok($"User added!");
            else
                return BadRequest("User could not get added to database!");
        }

        #endregion

        #region LastListenSong Specific

        [HttpPost("LastListenedSongs")]
        public async Task<ActionResult> AddLastListenSong([FromBody] SimpleLastListenedSong lastListenedSong)
        {
            if (await _databaseService.AddLastListenSongForUser(lastListenedSong))
                return Ok("Song Added");
            else
                return BadRequest("Song could not get added");
        }

        [HttpGet("LastListenedSongs")]
        public async Task <ActionResult <List<SongDto>>> GetLastListenedSongsOfUser(Guid userId, int amount)
        {
            var songs = await _databaseService.GetLastListenedSongsOfUser(userId, amount);

            if (songs == null)
                return BadRequest("User was not found");

            return Ok(songs);
        }

        #endregion

        #region Database Song Specific

        [HttpPost("DatabaseSongs")]
        public async Task <ActionResult> AddDatabaseSong([FromBody] DatabaseSong song)
        {
            if (await _databaseService.InsertSongIntoDatabase(song))
                return Ok("Song Added");
            else
                return BadRequest("Song could not get added");
        }

        [HttpGet("DatabaseSongs/{id}")]
        public async Task <ActionResult <DatabaseSong>> GetDatabaseSong(string id)
        {
            DatabaseSong song = await _databaseService.GetSongFromDatabase(id);
            
            Console.WriteLine($"Getting song for id {id}");
            
            if (song == null)
                return BadRequest($"No song was found with the id {id}");

            Console.WriteLine($"Found song {song}");
            
            return Ok(song);
        }

        #endregion
        
        #region Database Song Data Specific

        [HttpGet("SongData_Embed")]
        public async Task <ActionResult <string>> GetEmbedOfSong(string id)
        {
            var embed = await _databaseService.GetEmbedOfSong(id);

            if (embed == null)
                return BadRequest("No embed was found for the provided id");

            return Ok(embed);
        }

        #endregion
    }
}
