using backend_csharp.Database;
using backend_csharp.Helper;
using backend_csharp.Models;
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

        public async Task <bool> InsertUserToDatabase(SimpleUser simpleUser)
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

        #region LastListenedSong Specific

        public async Task <bool> AddLastListenSongForUser(SimpleLastListenedSong simpleLastListenedSong)
        {
            try
            {
                var lastListenedSong = simpleLastListenedSong.ToLastListenedSong();
                await _context.LastListenedSongs.AddAsync(lastListenedSong);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                return false;
            }

            return true;
        }

        public async Task <List <SongDto>> GetLastListenedSongsOfUser(Guid userId, int amount)
        {
            var user = await _context.Users
                                     .Include(x => x.LastListenedSongs)
                                     .ThenInclude(x => x.DatabaseSong)
                                     .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return null;

            // Order the results and only pick the wanted amount
            List <LastListenedSong> dbSongs = user.LastListenedSongs
                                                  .OrderBy(x => x.LastListenedTo)
                                                  .Take(amount)
                                                  .ToList();
            
            List<SongDto> lastListenedSongs = new List<SongDto>();



            foreach (LastListenedSong lastListenedSong in dbSongs)
            {
                if(lastListenedSong?.DatabaseSong == null)
                    continue;

                var song = await OpenSearchService.Instance.FindSongById(lastListenedSong.DatabaseSong.Id);

                if(song == null)
                    continue;

                lastListenedSongs.Add(song.ToSongDto());
            }

            return lastListenedSongs;
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
                                     .Include(x => x.LastListenedSongs)
                                     .FirstOrDefaultAsync(x => x.Id.Equals(songId));

            return song;
        }

        #endregion
    }
}
