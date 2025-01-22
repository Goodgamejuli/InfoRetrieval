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
        Album? foundAlbum;

        if (album.Id.StartsWith("mbid_"))
        {
            foundAlbum = await dataContext.Albums.FirstOrDefaultAsync(
                x => x.Name.Equals(album.Name) || x.Id.Equals(album.Id));
        }
        else
        {
            foundAlbum = await dataContext.Albums.FirstOrDefaultAsync(x => x.Id.Equals(album.Id));

            if (foundAlbum == null)
            {
                foundAlbum = await dataContext.Albums.FirstOrDefaultAsync(
                    x => x.Id.StartsWith("mbid_") && x.Name.Equals(album.Name));

                if (foundAlbum != null)
                    foundAlbum = await ReplaceAlbumId(foundAlbum, album.Id);
            }
        }

        if (foundAlbum != null)
            return foundAlbum;

        await dataContext.Albums.AddAsync(album);
        await dataContext.SaveChangesAsync();

        return album;
    }

    public async Task <Artist> TryInsertOrGetExistingArtist(Artist artist)
    {
        Artist? foundArtist;

        if (artist.Id.StartsWith("mbid_"))
        {
            foundArtist = await dataContext.Artists.FirstOrDefaultAsync(
                x => x.Name.Equals(artist.Name) || x.Id.Equals(artist.Id));
        }
        else
        {
            foundArtist = await dataContext.Artists.FirstOrDefaultAsync(x => x.Id.Equals(artist.Id));

            if (foundArtist == null)
            {
                foundArtist = await dataContext.Artists.FirstOrDefaultAsync(
                    x => x.Id.StartsWith("mbid_") && x.Name.Equals(artist.Name));

                if (foundArtist != null)
                    foundArtist = await ReplaceArtistId(foundArtist, artist.Id);
            }
        }

        if (foundArtist != null)
            return foundArtist;

        await dataContext.Artists.AddAsync(artist);
        await dataContext.SaveChangesAsync();

        return artist;
    }

    public async Task <Tuple <DatabaseSong, string?>> TryInsertOrGetExistingSong(DatabaseSong song)
    {
        DatabaseSong? foundSong;

        string? oldId = null;

        if (song.Id.StartsWith("mbid_"))
        {
            foundSong = await dataContext.DatabaseSongs.FirstOrDefaultAsync(
                x => x.Title.Equals(song.Title) || x.Id.Equals(song.Id));
        }
        else
        {
            foundSong = await dataContext.DatabaseSongs.FirstOrDefaultAsync(x => x.Id.Equals(song.Id));

            if (foundSong == null)
            {
                foundSong = await dataContext.DatabaseSongs.FirstOrDefaultAsync(
                    x => x.Id.StartsWith("mbid_") && x.Title.Equals(song.Title));

                if (foundSong != null)
                {
                    oldId = foundSong.Id;

                    Console.WriteLine("Found identical song in db... removing now!");
                    
                    dataContext.DatabaseSongs.Remove(foundSong);
                    foundSong = null;
                }
            }
        }

        if (foundSong != null)
            return new Tuple <DatabaseSong, string?>(foundSong, oldId);

        await dataContext.DatabaseSongs.AddAsync(song);
        await dataContext.SaveChangesAsync();

        return new Tuple <DatabaseSong, string?>(song, oldId);
    }

    #endregion

    #region Private Methods

    private static void ClearDbSet <T>(DbSet <T> entries) where T : class
    {
        foreach (T entity in entries)
            entries.Remove(entity);
    }

    private async Task <Album?> ReplaceAlbumId(Album album, string newId)
    {
        Album? oldAlbum =
            await dataContext.Albums.Include(x => x.Songs).FirstOrDefaultAsync(x => x.Id.Equals(album.Id));

        if (oldAlbum == null)
            return null;

        var newAlbum = new Album
        {
            Id = newId,
            Name = oldAlbum.Name,
            ArtistId = oldAlbum.ArtistId,
            CoverUrl = oldAlbum.CoverUrl,
            Songs = oldAlbum.Songs
        };

        foreach (DatabaseSong song in oldAlbum.Songs)
            song.AlbumId = newAlbum.Id;

        dataContext.Albums.Remove(oldAlbum);
        dataContext.Albums.Add(newAlbum);

        await dataContext.SaveChangesAsync();

        return newAlbum;
    }

    private async Task <Artist?> ReplaceArtistId(Artist artist, string newId)
    {
        Artist? oldArtist = await dataContext.Artists.Include(x => x.Albums).
                                              FirstOrDefaultAsync(x => x.Id.Equals(artist.Id));

        if (oldArtist == null)
            return null;

        var newArtist = new Artist
        {
            Id = newId, Name = oldArtist.Name, CoverUrl = oldArtist.CoverUrl, Albums = oldArtist.Albums
        };

        foreach (Album album in oldArtist.Albums)
            album.ArtistId = newArtist.Id;

        dataContext.Artists.Remove(oldArtist);
        dataContext.Artists.Add(newArtist);

        await dataContext.SaveChangesAsync();

        return newArtist;
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

    public async Task <DatabaseSong?> InsertSongIntoDatabase(DatabaseSong song)
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
