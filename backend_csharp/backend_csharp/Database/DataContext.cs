using backend_csharp.Models.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backend_csharp.Database
{
    public class DataContext : DbContext
    {
        public DbSet <User> Users { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            
        }



    }
}
