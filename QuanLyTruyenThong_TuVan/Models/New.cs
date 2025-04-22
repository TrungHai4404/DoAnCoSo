using System.ComponentModel.DataAnnotations;

namespace QuanLyTruyenThong_TuVan.Models
{
    public class New
    {
        public int Id { get; set; }

        [StringLength(255)]
        public string Title { get; set; }

        public string Notes { get; set; }

        public string Content { get; set; }

        [StringLength(255)]
        public string ImagesUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? SendAt { get; set; }

        public int SenderId { get; set; }
        public ApplicationResident Sender { get; set; }

        [StringLength(50)]
        public string Category { get; set; }

        [StringLength(50)]
        public string Status { get; set; }
    }
}
