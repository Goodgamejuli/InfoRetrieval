using backend_csharp.Database;
using backend_csharp.Helper;
using backend_csharp.Models;
using backend_csharp.Models.Database;
using backend_csharp.Models.Database.DTOs;
using Microsoft.EntityFrameworkCore;

namespace backend_csharp.Services;

/// <summary>
///     This class handles all functionality related to the database
/// </summary>
public class DatabaseService(DataContext dataContext)
{
    #region Public Methods

    /// <summary>
    ///     This method resets the local database
    /// </summary>
    /// <param name="clearSongs"> If true all songs are cleared </param>
    /// <param name="clearArtists"> If true all artists are cleared </param>
    /// <param name="clearAlbums"> If true all albums are cleared </param>
    /// <param name="clearLastListenedSongs"> If true all last listened songs are cleared </param>
    /// <param name="clearUsers"> If true all users are cleared </param>
    /// <param name="clearPlaylists"> If true all playlists are cleared </param>
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

    /// <summary>
    ///     This method returns the album with the specified id from the database
    /// </summary>
    /// <param name="id"> ID of the target album </param>
    public async Task <Album?> GetAlbum(string id)
    {
        return await dataContext.Albums.Include(x => x.Artist).FirstOrDefaultAsync(x => x.Id.Equals(id));
    }

    /// <summary>
    ///     This method returns the album of the specified song from the database
    /// </summary>
    /// <param name="songId"> ID of the target song </param>
    /// <returns></returns>
    public async Task <Album?> GetAlbumBySong(string songId)
    {
        DatabaseSong? song = await dataContext.DatabaseSongs.Include(x => x.Album).
                                               ThenInclude(x => x.Artist).
                                               FirstOrDefaultAsync(x => x.Id.Equals(songId));

        return song?.Album;
    }

    /// <summary>
    ///     This method returns all albums in the database
    /// </summary>
    public async Task <List <Album>> GetAllAlbums()
    {
        return await dataContext.Albums.ToListAsync();
    }

    /// <summary>
    ///     This method returns all songs corresponding to the specified album from the database
    /// </summary>
    /// <param name="albumId"> ID of the target album </param>
    public async Task <List <DatabaseSong>> GetAllAlbumSongs(string albumId)
    {
        Album? album = await dataContext.Albums.Include(album => album.Songs).
                                         FirstOrDefaultAsync(x => x.Id == albumId);

        return album == null ? [] : album.Songs.ToList();
    }

    /// <summary>
    ///     This method returns all albums corresponding to the specified artist from the database
    /// </summary>
    /// <param name="artistId"> ID of the target artist </param>
    public async Task <List <Album>> GetAllArtistAlbums(string artistId)
    {
        Artist? artist = await dataContext.Artists.Include(artist => artist.Albums).
                                           FirstOrDefaultAsync(x => x.Id == artistId);

        return artist == null ? [] : artist.Albums.ToList();
    }

    /// <summary>
    ///     This method returns all artists in the database
    /// </summary>
    public async Task <List <Artist>> GetAllArtists()
    {
        return await dataContext.Artists.ToListAsync();
    }

    /// <summary>
    ///     This method returns all songs of a specified artist from the database
    /// </summary>
    /// <param name="artistId"> ID of the target artist </param>
    public async Task <List <DatabaseSong>> GetAllArtistSongs(string artistId)
    {
        Artist? artist = await dataContext.Artists.Include(artist => artist.Albums).
                                           ThenInclude(album => album.Songs).
                                           FirstOrDefaultAsync(x => x.Id == artistId);

        return artist == null ? [] : artist.Albums.SelectMany(album => album.Songs).ToList();
    }

    /// <summary>
    ///     This method returns the artist corresponding to the id from the database
    /// </summary>
    /// <param name="id"> ID of the target artist </param>
    public async Task <Artist?> GetArtist(string id)
    {
        return await dataContext.Artists.FirstOrDefaultAsync(x => x.Id.Equals(id));
    }

    /// <summary>
    ///     This method returns the artist of the specified song from the database
    /// </summary>
    /// <param name="songId"> ID of the target song </param>
    public async Task <Artist?> GetArtistBySong(string songId)
    {
        DatabaseSong? song = await dataContext.DatabaseSongs.Include(x => x.Album).
                                               ThenInclude(x => x.Artist).
                                               FirstOrDefaultAsync(x => x.Id.Equals(songId));

        return song?.Album.Artist;
    }

    #region UserSpecific

    /// <summary>
    ///     This method adds a user to the database
    /// </summary>
    /// <param name="simpleUser"> Data for the new user </param>
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

    /// <summary>
    ///     This method adds a new album to the database if none with the name and artist already exists
    ///     If an album with the same name and artist exists but only has musicBrainz data and the new album contains
    ///     spotify data the old entry is overwritten
    /// </summary>
    /// <param name="album"> Album to save to the database </param>
    /// <returns> The album added or collected from the database </returns>
    public async Task <Album> TryInsertOrGetExistingAlbum(Album album)
    {
        Album? foundAlbum;

        if (album.Id.StartsWith("mbid_"))
        {
            // If the new album only has mb data we only need to check if an album with the mb data already exists
            foundAlbum = await dataContext.Albums.FirstOrDefaultAsync(
                x => x.Name!.Equals(album.Name) || x.Id.Equals(album.Id));
        }
        else
        {
            // If the new album contains spotify data we check if an album with that spotify data already exists
            foundAlbum = await dataContext.Albums.FirstOrDefaultAsync(x => x.Id.Equals(album.Id));

            if (foundAlbum == null)
            {
                // If none album with that spotify data exists we check if there is one with mb data that has the same name and artist
                foundAlbum = await dataContext.Albums.FirstOrDefaultAsync(
                    x => x.Id.StartsWith("mbid_") && x.Name!.Equals(album.Name));

                // If we did find a mb entry we replace that with the spotify entry
                if (foundAlbum != null)
                    foundAlbum = await ReplaceAlbum(foundAlbum, album);
            }
        }

        if (foundAlbum != null)
            return foundAlbum;

        // If none existing album was found we create an entry for the given album
        await dataContext.Albums.AddAsync(album);
        await dataContext.SaveChangesAsync();

        return album;
    }

    /// <summary>
    ///     This method adds a new artist to the database if none with the name already exists
    ///     If an artist with the same name exists but only has musicBrainz data and the new artist contains
    ///     spotify data the old entry is overwritten
    /// </summary>
    /// <param name="artist"> Artist to save to the database </param>
    /// <returns> The artist added or collected from the database </returns>
    public async Task <Artist> TryInsertOrGetExistingArtist(Artist artist)
    {
        Artist? foundArtist;

        if (artist.Id.StartsWith("mbid_"))
        {
            // If the new artist only has mb data we only need to check if an artist with the mb data already exists
            foundArtist = await dataContext.Artists.FirstOrDefaultAsync(
                x => x.Name!.Equals(artist.Name) || x.Id.Equals(artist.Id));
        }
        else
        {
            // If the new artist contains spotify data we check if an artist with that spotify data already exists
            foundArtist = await dataContext.Artists.FirstOrDefaultAsync(x => x.Id.Equals(artist.Id));

            if (foundArtist == null)
            {
                // If none artist with that spotify data exists we check if there is one with mb data that has the same name
                foundArtist = await dataContext.Artists.FirstOrDefaultAsync(
                    x => x.Id.StartsWith("mbid_") && x.Name!.Equals(artist.Name));

                // If we did find a mb entry we replace that with the spotify entry
                if (foundArtist != null)
                    foundArtist = await ReplaceArtist(foundArtist, artist);
            }
        }

        if (foundArtist != null)
            return foundArtist;

        // If none existing artist was found we create an entry for the given artist
        await dataContext.Artists.AddAsync(artist);
        await dataContext.SaveChangesAsync();

        return artist;
    }

    /// <summary>
    ///     This method adds a new song to the database if none with the name already exists
    ///     If a song with the same name exists but only has musicBrainz data and the new song contains
    ///     spotify data the old entry is overwritten
    /// </summary>
    /// <param name="song"> Song to save to the database </param>
    /// <returns> The song added or collected from the database </returns>
    public async Task <Tuple <DatabaseSong, string?>> TryInsertOrGetExistingSong(DatabaseSong song)
    {
        DatabaseSong? foundSong;

        string? oldId = null;

        if (song.Id.StartsWith("mbid_"))
        {
            // If the new song only has mb data we only need to check if a song with the mb data already exists
            foundSong = await dataContext.DatabaseSongs.FirstOrDefaultAsync(
                x => x.Title.Equals(song.Title) || x.Id.Equals(song.Id));
        }
        else
        {
            // If the new song contains spotify data we check if a song with that spotify data already exists
            foundSong = await dataContext.DatabaseSongs.FirstOrDefaultAsync(x => x.Id.Equals(song.Id));

            if (foundSong == null)
            {
                // If no song with that spotify data exists we check if there is one with mb data that has the same name
                foundSong = await dataContext.DatabaseSongs.FirstOrDefaultAsync(
                    x => x.Id.StartsWith("mbid_") && x.Title.Equals(song.Title));

                if (foundSong != null)
                {
                    oldId = foundSong.Id;

                    Console.WriteLine("Found identical song in db... removing now!");

                    // If we did find a mb entry we remove it from the database
                    dataContext.DatabaseSongs.Remove(foundSong);

                    // We reset the found song to null so we do not use the early return, but add the new song to the database
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

    /// <summary>
    ///     Utility method to clear a specific database set
    /// </summary>
    /// <param name="entries"> Set to be cleared </param>
    private static void ClearDbSet <T>(DbSet <T> entries) where T : class
    {
        foreach (T entity in entries)
            entries.Remove(entity);
    }

    /// <summary>
    ///     Utility method to replace an album entry with a new one
    /// </summary>
    /// <param name="album"> Album entry to replace </param>
    /// <param name="newAlbum"> New album to replace the old one with </param>
    private async Task <Album?> ReplaceAlbum(Album album, Album newAlbum)
    {
        Album? oldAlbum =
            await dataContext.Albums.Include(x => x.Songs).FirstOrDefaultAsync(x => x.Id.Equals(album.Id));

        if (oldAlbum == null)
            return null;

        newAlbum.Songs = oldAlbum.Songs;

        // Replace the relations on the corresponding songs
        foreach (DatabaseSong song in oldAlbum.Songs)
            song.AlbumId = newAlbum.Id;

        dataContext.Albums.Remove(oldAlbum);
        dataContext.Albums.Add(newAlbum);

        await dataContext.SaveChangesAsync();

        return newAlbum;
    }

    /// <summary>
    ///     Utility method to replace an artist entry with a new one
    /// </summary>
    /// <param name="artist"> Artist entry to replace </param>
    /// <param name="newArtist"> New artist to replace the old one with </param>
    private async Task <Artist?> ReplaceArtist(Artist artist, Artist newArtist)
    {
        Artist? oldArtist = await dataContext.Artists.Include(x => x.Albums).
                                              FirstOrDefaultAsync(x => x.Id.Equals(artist.Id));

        if (oldArtist == null)
            return null;

        newArtist.Albums = oldArtist.Albums;

        // Replace the relations on the corresponding albums
        foreach (Album album in oldArtist.Albums)
            album.ArtistId = newArtist.Id;

        dataContext.Artists.Remove(oldArtist);
        dataContext.Artists.Add(newArtist);

        await dataContext.SaveChangesAsync();

        return newArtist;
    }

    #endregion

    #region LastListenedSong Specific

    /// <summary>
    ///     This method adds a song as a last listened song
    /// </summary>
    /// <param name="simpleLastListenedSong"> Song Data </param>
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

    /// <summary>
    ///     This method returns all last listened songs of a specified user from the db
    /// </summary>
    /// <param name="userId"> ID of the target user </param>
    /// <param name="amount"> Amount of the returned songs </param>
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

    /// <summary>
    ///     This method returns a song corresponding to the specified id from the database
    /// </summary>
    /// <param name="songId"> ID of the target song </param>
    public async Task <DatabaseSong?> GetSong(string songId)
    {
        DatabaseSong? song = await dataContext.DatabaseSongs.Include(x => x.LastListenedSongs).
                                               Include(x => x.Album).
                                               ThenInclude(album => album.Artist).
                                               FirstOrDefaultAsync(x => x.Id.Equals(songId));

        return song;
    }

    /// <summary>
    ///     This method returns all songs from the database
    /// </summary>
    public async Task <List <DatabaseSong>> GetAllSongs()
    {
        return await dataContext.DatabaseSongs.Include(song => song.Album).ToListAsync();
    }

    #endregion
}
