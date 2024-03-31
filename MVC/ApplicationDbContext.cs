using Microsoft.EntityFrameworkCore;
using MVC.Models.Entity;

namespace MVC
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
