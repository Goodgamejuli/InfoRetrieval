namespace backend_csharp.Models.Database.DTOs;

/// <summary>
///     This class is a data transfer object for songs the user last listened to
/// </summary>
public class SimpleLastListenedSong
{
    public Guid UserId {get; set;}

    public string DatabaseSongId {get; set;} = null!;
}
