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

        // Các DbSet của bạn
        public DbSet<Apartment> Apartments { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<Response> Responses { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<New> News { get; set; } = null!;
        public DbSet<Vote> Votes { get; set; } = null!;
        public DbSet<VoteOption> VoteOptions { get; set; } = null!;
        public DbSet<VoteResult> VoteResults { get; set; } = null!;
        public DbSet<ApplicationResident> ApplicationResident { get; set; } = null!;   
        public DbSet<Forum> Forums { get; set; } = null!;   // Thêm bảng Forum
        public DbSet<ForumPost> ForumPosts { get; set; } = null!;   // Thêm bảng ForumPost

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // VoteResult ↔ VoteOption, Vote
            builder.Entity<VoteResult>()
                .HasOne(vr => vr.VoteOption)
                .WithMany(vo => vo.VoteResults)
                .HasForeignKey(vr => vr.VoteOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<VoteResult>()
                .HasOne(vr => vr.Vote)
                .WithMany(v => v.VoteResults)
                .HasForeignKey(vr => vr.VoteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Response ↔ Comment, Resident
            builder.Entity<Response>()
                .HasOne(r => r.Comment)
                .WithMany(c => c.Responses)
                .HasForeignKey(r => r.CommentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Comment>()
                .HasOne(c => c.Resident)
                .WithMany() // nếu cần navigation ngược thì đổi .WithMany(r => r.Comments)
                .HasForeignKey(c => c.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Response>()
                .HasOne(r => r.Resident)
                .WithMany() // nếu cần navigation ngược thì đổi .WithMany(r => r.Responses)
                .HasForeignKey(r => r.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ForumPost ↔ Forum
            builder.Entity<ForumPost>()
                .HasOne(fp => fp.Forum)
                .WithMany()   // nếu Forum có ICollection<ForumPost> Posts thì đổi thành .WithMany(f => f.Posts)
                .HasForeignKey(fp => fp.TopicId)
                .OnDelete(DeleteBehavior.Restrict);

            // (tùy chọn) nếu muốn thiết lập quan hệ 1-1 giữa Forum.PostId và ForumPost.Id:
            // builder.Entity<Forum>()
            //     .HasOne<ForumPost>()
            //     .WithOne()
            //     .HasForeignKey<Forum>(f => f.PostId)
            //     .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
