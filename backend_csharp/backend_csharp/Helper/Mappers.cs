using backend_csharp.Models;
using backend_csharp.Models.Database;
using backend_csharp.Models.Database.DTOs;

namespace backend_csharp.Helper
{
    public static class Mappers
    {
        public static User ToUser(this SimpleUser simpleUser)
        {
            return new User()
            {
                Id = Guid.NewGuid(),
                Username = simpleUser.Username,
                Password = simpleUser.Password
            };
        }

        public static LastListenedSong ToLastListenedSong(this SimpleLastListenedSong simpleSong)
        {
            return new LastListenedSong()
            {
                Id = Guid.NewGuid(), UserId = simpleSong.UserId, DatabaseSongId = simpleSong.DatabaseSongId
            };
        }
    }
}
