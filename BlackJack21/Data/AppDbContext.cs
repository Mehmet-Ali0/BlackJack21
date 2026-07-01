using BlackJack21.Models;
using Microsoft.EntityFrameworkCore;

namespace BlackJack21.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Game> Games { get; set; }

        public DbSet<Player> Players { get; set; }

        public DbSet<Hand> Hands { get; set; }

        public DbSet<Card> Cards { get; set; }


    }
}
