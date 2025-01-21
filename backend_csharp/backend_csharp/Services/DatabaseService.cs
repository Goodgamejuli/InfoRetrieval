using backend_csharp.Database;
using backend_csharp.Helper;
using backend_csharp.Models;
using backend_csharp.Models.Database;
using backend_csharp.Models.Database.DTOs;
using Microsoft.EntityFrameworkCore;

namespace backend_csharp.Services;

public class DatabaseService(DataContext dataContext)
{
    #region Public Methods

    public async Task ClearDatabase(
        bool clearSongs,
        bool clearArtists,
        bool clearAlbums,
        bool clearLastListenedSongs,
        bool clearUsers,
        bool clearPlaylists)
    {
        if (clearSongs)
            ClearDbSet(dataContext.DatabaseSongs);

        if (clearArtists)
            ClearDbSet(dataContext.Artists);

        if (clearAlbums)
            ClearDbSet(dataContext.Albums);

        if (clearLastListenedSongs)
            ClearDbSet(dataContext.LastListenedSongs);

        if (clearUsers)
            ClearDbSet(dataContext.Users);

        if (clearPlaylists)
            ClearDbSet(dataContext.Playlists);

        await dataContext.SaveChangesAsync();
    }

    public async Task <Album?> GetAlbum(string id)
    {
        return await dataContext.Albums.FirstOrDefaultAsync(x => x.Id.Equals(id));
    }

    public async Task <List <Album>> GetAllAlbums()
    {
        return await dataContext.Albums.ToListAsync();
    }

    public async Task <List <DatabaseSong>> GetAllAlbumSongs(string albumId)
    {
        Album? album = await dataContext.Albums.Include(album => album.Songs).
                                         FirstOrDefaultAsync(x => x.Id == albumId);

        return album == null ? [] : album.Songs.ToList();
    }

    public async Task <List <Album>> GetAllArtistAlbums(string artistId)
    {
        Artist? artist = await dataContext.Artists.Include(artist => artist.Albums).
                                           FirstOrDefaultAsync(x => x.Id == artistId);

        return artist == null ? [] : artist.Albums.ToList();
    }

    public async Task <List <Artist>> GetAllArtists()
    {
        return await dataContext.Artists.ToListAsync();
    }

    public async Task <List <DatabaseSong>> GetAllArtistSongs(string artistId)
    {
        Artist? artist = await dataContext.Artists.Include(artist => artist.Albums).
                                           ThenInclude(album => album.Songs).
                                           FirstOrDefaultAsync(x => x.Id == artistId);

        return artist == null ? [] : artist.Albums.SelectMany(album => album.Songs).ToList();
    }

    public async Task <Artist?> GetArtist(string id)
    {
        return await dataContext.Artists.FirstOrDefaultAsync(x => x.Id.Equals(id));
    }

    #region UserSpecific

    public async Task <bool> InsertUserToDatabase(SimpleUser simpleUser)
    {
        try
        {
            var user = simpleUser.ToUser();
            await dataContext.Users.AddAsync(user);
            await dataContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            return false;
        }

        return true;
    }

    #endregion

    public async Task <Album> TryInsertOrGetExistingAlbum(Album album)
    {
        Album? foundAlbum = await dataContext.Albums.FirstOrDefaultAsync(x => x.Id == album.Id);

        if (foundAlbum != null)
            return foundAlbum;

        await dataContext.Albums.AddAsync(album);
        await dataContext.SaveChangesAsync();

        return album;
    }

    public async Task <Artist> TryInsertOrGetExistingArtist(Artist artist)
    {
        Artist? foundArtist = await dataContext.Artists.FirstOrDefaultAsync(x => x.Id == artist.Id);

        if (foundArtist != null)
            return foundArtist;

        await dataContext.Artists.AddAsync(artist);
        await dataContext.SaveChangesAsync();

        return artist;
    }

    #endregion

    #region Private Methods

    private static void ClearDbSet <T>(DbSet <T> entries) where T : class
    {
        foreach (T entity in entries)
            entries.Remove(entity);
    }

    #endregion

    #region LastListenedSong Specific

    public async Task <bool> AddLastListenSongForUser(SimpleLastListenedSong simpleLastListenedSong)
    {
        try
        {
            var lastListenedSong = simpleLastListenedSong.ToLastListenedSong();
            await dataContext.LastListenedSongs.AddAsync(lastListenedSong);
            await dataContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            return false;
        }

        return true;
    }

    public async Task <List <SongDto>?> GetLastListenedSongsOfUser(Guid userId, int amount)
    {
        User? user = await dataContext.Users.Include(x => x.LastListenedSongs).
                                       ThenInclude(x => x.DatabaseSong).
                                       FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
            return null;

        // Order the results and only pick the wanted amount
        List <LastListenedSong> listenedSongs =
            user.LastListenedSongs.OrderBy(x => x.LastListenedTo).Take(amount).ToList();

        List <SongDto> lastListenedSongs = [];

        foreach (LastListenedSong listenedSong in listenedSongs)
        {
            OpenSearchSongDocument? osSong =
                await OpenSearchService.Instance.FindSongById(listenedSong.DatabaseSong.Id);

            DatabaseSong? dbSong = await GetSong(listenedSong.DatabaseSongId);

            if (osSong == null || dbSong == null)
                continue;

            lastListenedSongs.Add(new SongDto(osSong, dbSong));
        }

        return lastListenedSongs;
    }

    #endregion

    #region DatabaseSong Specific

    public async Task<DatabaseSong?> InsertSongIntoDatabase(DatabaseSong song)
    {
        try
        {
            await dataContext.DatabaseSongs.AddAsync(song);
            await dataContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            return null;
        }

        return song;
    }

    public async Task <DatabaseSong?> GetSong(string songId)
    {
        DatabaseSong? song = await dataContext.DatabaseSongs.Include(x => x.LastListenedSongs).
                                               Include(x => x.Album).
                                               FirstOrDefaultAsync(x => x.Id.Equals(songId));

        return song;
    }

    public async Task <List <DatabaseSong>> GetAllSongs()
    {
        return await dataContext.DatabaseSongs.Include(song => song.Album).ToListAsync();
    }

    #endregion
}
