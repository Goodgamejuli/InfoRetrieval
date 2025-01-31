using backend_csharp.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace backend_csharp.Database;

/// <summary>
///     This class defines the database used to store arbitrary data that is not saved on the openSearch instance
/// </summary>
public class DataContext(DbContextOptions <DataContext> options) : DbContext(options)
{
    /// <summary>
    ///     Stores all users
    /// </summary>
    public DbSet <User> Users {get; set;}

    /// <summary>
    ///     Stores all playlists created by users
    /// </summary>
    public DbSet <Playlist> Playlists {get; set;}

    /// <summary>
    ///     Stores all added albums
    /// </summary>
    public DbSet <Album> Albums {get; set;}

    /// <summary>
    ///     Stores all added artists
    /// </summary>
    public DbSet <Artist> Artists {get; set;}

    /// <summary>
    ///     Stores all last songs the users have listened to
    /// </summary>
    public DbSet <LastListenedSong> LastListenedSongs {get; set;}

    /// <summary>
    ///     Stores all added songs
    /// </summary>
    public DbSet <DatabaseSong> DatabaseSongs {get; set;}
}
