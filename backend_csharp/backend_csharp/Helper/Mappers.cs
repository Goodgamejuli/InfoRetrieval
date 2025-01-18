using backend_csharp.Models.Database;
using backend_csharp.Models.Database.DTOs;

namespace backend_csharp.Helper
{
    public static class Mappers
    {
        public static User ToUser(this SimpleUserDto simpleUser)
        {
            return new User()
            {
                Id = Guid.NewGuid(),
                Username = simpleUser.Username,
                Password = simpleUser.Password
            };
        }
    }
}
