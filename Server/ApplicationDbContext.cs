using Microsoft.EntityFrameworkCore;
using Server.Models.Entity;

namespace Server
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
    }
}
