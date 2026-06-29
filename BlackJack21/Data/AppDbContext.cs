using Microsoft.EntityFrameworkCore;

namespace BlackJack21.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }








    }
}
