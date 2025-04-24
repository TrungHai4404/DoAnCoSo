using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuanLyTruyenThong_TuVan.Models;
namespace QuanLyTruyenThong_TuVan.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationResident>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Apartment> Apartments { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Response> Responses { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<New> News { get; set; }

       
    }
}
