using backend_csharp.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace backend_csharp.Database;

public class DataContext : DbContext
{
    public DbSet <User> Users {get; set;}

    public DbSet <Playlist> Playlists {get; set;}

    public DbSet <Album> Albums {get; set;}

    public DbSet <Artist> Artists {get; set;}

    public DbSet <LastListenedSong> LastListenedSongs {get; set;}

    public DbSet <DatabaseSong> DatabaseSongs {get; set;}

    public DataContext(DbContextOptions <DataContext> options) : base(options)
    {
    }
}
