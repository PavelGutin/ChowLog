using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ChowLog.WebMVC.Models;

namespace ChowLog.WebMVC.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ChowLog.WebMVC.Models.Plate> Plate { get; set; } = default!;
    }
}
