using backend_csharp.Models;
using backend_csharp.Models.Database;
using backend_csharp.Models.Database.DTOs;
using backend_csharp.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend_csharp.Controllers;

/// <summary>
///     This api controller handles all api calls corresponding with the connected database
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class DatabaseController(DatabaseService service) : ControllerBase
{
    #region Public Methods

    #region DB_Users

    /// <summary>
    ///     This method allows to add a new user to the database
    /// </summary>
    /// <param name="user"> Defines the new user's data </param>
    [HttpPost("AddUser")]
    public async Task <ActionResult> AddNewUser([FromBody] SimpleUser user)
    {
        if (await service.InsertUserToDatabase(user))
            return Ok("User added!");

        return BadRequest("User could not get added to database!");
    }

    #endregion

    #endregion

    #region DB_Basics

    /// <summary>
    ///     Utility method used to check if this service (api) is reachable
    /// </summary>
    [HttpGet("IsReachable")]
    public Task <ActionResult> IsReachable()
    {
        return Task.FromResult <ActionResult>(Ok("Is reachable"));
    }

    /// <summary>
    ///     Utility method to reset the local database
    /// </summary>
    /// <param name="clearSongs"> If true all songs are cleared </param>
    /// <param name="clearArtists"> If true all artists are cleared </param>
    /// <param name="clearAlbums"> If true all albums are cleared </param>
    /// <param name="clearLastListenedSongs"> If true all last listened songs are cleared </param>
    /// <param name="clearUsers"> If true all users are cleared </param>
    /// <param name="clearPlaylists"> If true all playlists are cleared </param>
    [HttpDelete("Clear")]
    public async Task <ActionResult> ClearDatabase(
        bool clearSongs = true,
        bool clearArtists = true,
        bool clearAlbums = true,
        bool clearLastListenedSongs = true,
        bool clearUsers = false,
        bool clearPlaylists = false)
    {
        await service.ClearDatabase(
            clearSongs,
            clearArtists,
            clearAlbums,
            clearLastListenedSongs,
            clearUsers,
            clearPlaylists);

        return Ok();
    }

    #endregion

    #region DB_Songs

    /// <summary>
    ///     This method returns a song with the corresponding id in the form of a dto
    /// </summary>
    /// <param name="id"> ID of the target song </param>
    [HttpGet("GetSong")]
    public async Task <ActionResult <SongDto>> GetDatabaseSong(string id)
    {
        DatabaseSong? dbSong = await service.GetSong(id);

        if (dbSong == null)
            return BadRequest($"No song was found with the id [{id}]!");

        OpenSearchSongDocument? osSong = await OpenSearchService.Instance.FindSongById(id);

        if (osSong == null)
        {
            return BadRequest(
                $"The found db song with the id [{id
                }] has not been added to openSearch! Or OpenSearch is not running!");
        }

        return Ok(new SongDto(osSong, dbSong));
    }

    /// <summary>
    ///     This method returns all saved songs from the database as dto
    /// </summary>
    [HttpGet("GetAllSongs")]
    public async Task <ActionResult <SongDto[]>> GetAllDatabaseSongs()
    {
        List <DatabaseSong> songs = await service.GetAllSongs();

        if (songs.Count == 0)
            return BadRequest("No songs were found!");

        List <SongDto> songDtOs = [];

        foreach (DatabaseSong song in songs)
        {
            OpenSearchSongDocument? osSong = await OpenSearchService.Instance.FindSongById(song.Id);

            if (osSong == null)
            {
                return BadRequest(
                    $"The found db song with the id [{song.Id
                    }] has not been added to openSearch! Or OpenSearch is not running!");
            }

            songDtOs.Add(new SongDto(osSong, song));
        }

        return Ok(songDtOs.ToArray());
    }

    /// <summary>
    ///     This method returns all songs of a specidied album as dto
    /// </summary>
    /// <param name="albumId"> ID of the target album </param>
    [HttpGet("GetAllAlbumSongs")]
    public async Task <ActionResult <SongDto[]>> GetAllAlbumSongs(string albumId)
    {
        Album? album = await service.GetAlbum(albumId);

        if (album == null)
            return BadRequest($"No album found for the id [{albumId}]!");

        List <DatabaseSong> songs = await service.GetAllAlbumSongs(albumId);

        if (songs.Count == 0)
            return BadRequest($"No songs were found for the album [{album.Name}]!");

        List <SongDto> songDtOs = [];

        foreach (DatabaseSong song in songs)
        {
            OpenSearchSongDocument? osSong = await OpenSearchService.Instance.FindSongById(song.Id);

            if (osSong == null)
            {
                return BadRequest(
                    $"The found db song with the id [{song.Id
                    }] has not been added to openSearch! Or OpenSearch is not running!");
            }

            songDtOs.Add(new SongDto(osSong, song));
        }

        return Ok(songDtOs.ToArray());
    }

    /// <summary>
    ///     This method returns all songs of a certain artist as dto
    /// </summary>
    /// <param name="artistId"> ID of the target artist </param>
    [HttpGet("GetAllArtistSongs")]
    public async Task <ActionResult <SongDto[]>> GetAllArtistSongs(string artistId)
    {
        Artist? artist = await service.GetArtist(artistId);

        if (artist == null)
            return BadRequest($"No artist found for the id [{artistId}]!");

        List <DatabaseSong> songs = await service.GetAllArtistSongs(artistId);

        if (songs.Count == 0)
            return BadRequest($"No songs were found for the artist [{artist.Name}]!");

        List <SongDto> songDtOs = [];

        foreach (DatabaseSong song in songs)
        {
            OpenSearchSongDocument? osSong = await OpenSearchService.Instance.FindSongById(song.Id);

            if (osSong == null)
            {
                return BadRequest(
                    $"The found db song with the id [{song.Id
                    }] has not been added to openSearch! Or OpenSearch is not running!");
            }

            songDtOs.Add(new SongDto(osSong, song));
        }

        return Ok(songDtOs.ToArray());
    }

    #endregion

    #region DB_Albums

    /// <summary>
    ///     This method returns the corresponding album as a dto
    /// </summary>
    /// <param name="id"> ID of the target album </param>
    [HttpGet("GetAlbum")]
    public async Task <ActionResult <Album>> GetDatabaseAlbum(string id)
    {
        Album? album = await service.GetAlbum(id);

        if (album == null)
            return BadRequest($"No album found for the id [{id}]!");

        return Ok(album);
    }

    /// <summary>
    ///     This method returns all albums saved in the database
    /// </summary>
    [HttpGet("GetAllAlbums")]
    public async Task <ActionResult <Album[]>> GetAllDatabaseAlbums()
    {
        List <Album> albums = await service.GetAllAlbums();

        if (albums.Count == 0)
            return BadRequest("No albums were found!");

        return Ok(albums.ToArray());
    }

    /// <summary>
    ///     This method returns all albums of a certain artist
    /// </summary>
    /// <param name="artistId"> ID of the target artist </param>
    [HttpGet("GetAllArtistAlbums")]
    public async Task <ActionResult <Album[]>> GetAllArtistAlbums(string artistId)
    {
        Artist? artist = await service.GetArtist(artistId);

        if (artist == null)
            return BadRequest($"No artist found for the id [{artistId}]!");

        List <Album> albums = await service.GetAllArtistAlbums(artistId);

        if (albums.Count == 0)
            return BadRequest($"No albums were found for the artist [{artist.Name}]!");

        return Ok(albums.ToArray());
    }

    #endregion

    #region DB_Artists

    /// <summary>
    ///     This method returns an artist
    /// </summary>
    /// <param name="id"> ID of the target artist </param>
    [HttpGet("GetArtist")]
    public async Task <ActionResult <Artist>> GetAllDatabaseArtist(string id)
    {
        Artist? artist = await service.GetArtist(id);

        if (artist == null)
            return BadRequest($"No artist found for the id [{id}]!");

        return Ok(artist);
    }

    /// <summary>
    ///     This method returns all artists saved in the database
    /// </summary>
    [HttpGet("GetAllArtists")]
    public async Task <ActionResult <Artist[]>> GetAllDatabaseArtists()
    {
        List <Artist> artists = await service.GetAllArtists();

        if (artists.Count == 0)
            return BadRequest("No artists were found!");

        return Ok(artists.ToArray());
    }

    #endregion

    #region DB_LastListening

    [HttpPost("AddLastListenedSong")]
    public async Task <ActionResult> AddLastListenSong([FromBody] SimpleLastListenedSong lastListenedSong)
    {
        if (await service.AddLastListenSongForUser(lastListenedSong))
            return Ok("Song added!");

        return BadRequest("Song could not get added!");
    }

    [HttpGet("GetLastListenedSongs")]
    public async Task <ActionResult <SongDto[]>> GetLastListenedSongsOfUser(Guid userId, int amount)
    {
        List <SongDto>? songs = await service.GetLastListenedSongsOfUser(userId, amount);

        if (songs == null)
            return BadRequest("User was not found!");

        return Ok(songs.ToArray());
    }

    #endregion
}
