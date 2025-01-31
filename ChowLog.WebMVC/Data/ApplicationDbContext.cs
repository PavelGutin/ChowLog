using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AllYourPlates.WebMVC.Models;

namespace AllYourPlates.WebMVC.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<AllYourPlates.WebMVC.Models.Plate> Plate { get; set; } = default!;
    }
}
