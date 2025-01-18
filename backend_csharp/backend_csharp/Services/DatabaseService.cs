using backend_csharp.Database;
using backend_csharp.Helper;
using backend_csharp.Models.Database;
using backend_csharp.Models.Database.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_csharp.Services
{
    public class DatabaseService
    {
        private readonly DataContext _context;

        public DatabaseService(DataContext dataContext)
        {
            _context = dataContext;
        }

        #region UserSpecific

        public async Task <bool> InsertUserToDatabase(SimpleUserDto simpleUser)
        {
            try
            {
                var user = simpleUser.ToUser();
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                return false;
            }

            return true;
        }

        #endregion

        #region DatabaseSong Specific

        public async Task<bool> InsertSongIntoDatabase(DatabaseSong song)
        {
            try
            {
                await _context.DatabaseSongs.AddAsync(song);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                return false;
            }

            return true;
        }

        public async Task <DatabaseSong> GetSongFromDatabase(string songId)
        {
            var song = await _context.DatabaseSongs
                                     .FirstOrDefaultAsync(x => x.Id.Equals(songId));

            return song;
        }

        #endregion
    }
}
