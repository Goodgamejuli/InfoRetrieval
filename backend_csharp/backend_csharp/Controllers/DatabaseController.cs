using backend_csharp.Models;
using backend_csharp.Models.Database;
using backend_csharp.Models.Database.DTOs;
using backend_csharp.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend_csharp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DatabaseController(DatabaseService service) : ControllerBase
{
    #region DB_Users

    [HttpPost("AddUser")]
    public async Task <ActionResult> AddNewUser([FromBody] SimpleUser user)
    {
        if (await service.InsertUserToDatabase(user))
            return Ok("User added!");

        return BadRequest("User could not get added to database!");
    }

    #endregion

    #region DB_Basics

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

    [HttpGet("GetSong")]
    public async Task <ActionResult <SongDto>> GetDatabaseSong(string id)
    {
        DatabaseSong? dbSong = await service.GetSong(id);

        if (dbSong == null)
            return BadRequest($"No song was found with the id [{id}]!");

        OpenSearchSongDocument? osSong = await OpenSearchService.Instance.FindSongById(id);

        if (osSong == null)
            return BadRequest(
                $"The found db song with the id [{id
                }] has not been added to openSearch! Or OpenSearch is not running!");

        return Ok(new SongDto(osSong, dbSong));
    }

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
                return BadRequest(
                    $"The found db song with the id [{song.Id
                    }] has not been added to openSearch! Or OpenSearch is not running!");
            
            songDtOs.Add(new SongDto(osSong, song));
        }
        
        return Ok(songDtOs.ToArray());
    }

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
                return BadRequest(
                    $"The found db song with the id [{song.Id
                    }] has not been added to openSearch! Or OpenSearch is not running!");
            
            songDtOs.Add(new SongDto(osSong, song));
        }

        return Ok(songDtOs.ToArray());
    }

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
                return BadRequest(
                    $"The found db song with the id [{song.Id
                    }] has not been added to openSearch! Or OpenSearch is not running!");
            
            songDtOs.Add(new SongDto(osSong, song));
        }
        
        return Ok(songDtOs.ToArray());
    }

    #endregion

    #region DB_Albums

    [HttpGet("GetAlbum")]
    public async Task <ActionResult <Album>> GetDatabaseAlbum(string id)
    {
        Album? album = await service.GetAlbum(id);

        if (album == null)
            return BadRequest($"No album found for the id [{id}]!");

        return Ok(album);
    }

    [HttpGet("GetAllAlbums")]
    public async Task <ActionResult <Album[]>> GetAllDatabaseAlbums()
    {
        List <Album> albums = await service.GetAllAlbums();

        if (albums.Count == 0)
            return BadRequest("No albums were found!");

        return Ok(albums.ToArray());
    }

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

    [HttpGet("GetArtist")]
    public async Task <ActionResult <Artist>> GetAllDatabaseArtist(string id)
    {
        Artist? artist = await service.GetArtist(id);

        if (artist == null)
            return BadRequest($"No artist found for the id [{id}]!");

        return Ok(artist);
    }

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
