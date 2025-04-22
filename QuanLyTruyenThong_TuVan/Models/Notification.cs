using System.ComponentModel.DataAnnotations;

namespace QuanLyTruyenThong_TuVan.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [StringLength(255)]
        public string Title { get; set; }

        public string Content { get; set; }

        [StringLength(255)]
        public string ImageUrl { get; set; }

        [StringLength(50)]
        public string Type { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? SentAt { get; set; }

        public int SenderId { get; set; }
        public ApplicationResident Sender { get; set; }

        [StringLength(50)]
        public string TargetAudience { get; set; }
    }
}
