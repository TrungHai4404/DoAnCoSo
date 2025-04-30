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

        // Các DbSet hiện tại của bạn
        public DbSet<Apartment> Apartments { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Response> Responses { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<New> News { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<VoteOption> VoteOptions { get; set; }
        public DbSet<VoteResult> VoteResults { get; set; }
        public DbSet<ApplicationResident> ApplicationResidents { get; set; }
        public DbSet<Post> Posts { get; set; }

        // Cấu hình mối quan hệ trong OnModelCreating
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Đặt hành vi ON DELETE NO ACTION cho các mối quan hệ liên kết
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

            builder.Entity<Response>()
                .HasOne(r => r.Comment)
                .WithMany(c => c.Responses)
                .HasForeignKey(r => r.CommentId)
                .OnDelete(DeleteBehavior.Restrict); // tránh multiple cascade paths

            builder.Entity<Comment>()
                .HasOne(c => c.Resident)
                .WithMany() // giả sử không cần navigation ngược
                .HasForeignKey(c => c.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Response>()
                .HasOne(r => r.Resident)
                .WithMany() // giả sử không cần navigation ngược
                .HasForeignKey(r => r.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);

           
        }
    }
}
