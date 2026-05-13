using ExchangeRateDSP.Models;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRateDSP.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<ExchangeRate> ExchangeRates { get; set; }
        public DbSet<AppLog> Logs { get; set; }

        protected AppDbContext()
        {

        }
    }
}
