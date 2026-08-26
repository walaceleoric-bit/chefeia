using chefeia.Models;
using Microsoft.EntityFrameworkCore;

namespace chefeia.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Plan> Plans { get; set; }
    }
}