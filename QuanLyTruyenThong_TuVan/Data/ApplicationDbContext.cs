using Microsoft.AspNetCore.Identity;
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
        public DbSet<Vote> Votes { get; set; }
        public DbSet<VoteOption> VoteOptions { get; set; }
        public DbSet<VoteResult> VoteResults { get; set; }
        public object Categories { get; internal set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Đặt hành vi ON DELETE NO ACTION cho VoteResults liên kết với VoteOptions
            builder.Entity<VoteResult>()
                .HasOne(vr => vr.VoteOption)
                .WithMany(vo => vo.VoteResults)
                .HasForeignKey(vr => vr.VoteOptionId)
                .OnDelete(DeleteBehavior.Restrict); // Hoặc .OnDelete(DeleteBehavior.NoAction)

            builder.Entity<VoteResult>()
                .HasOne(vr => vr.Vote)
                .WithMany(v => v.VoteResults)
                .HasForeignKey(vr => vr.VoteId)
                .OnDelete(DeleteBehavior.Restrict); // Hoặc .OnDelete(DeleteBehavior.NoAction)

        }



    }

}
