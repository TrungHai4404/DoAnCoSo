using System.ComponentModel.DataAnnotations;

namespace QuanLyTruyenThong_TuVan.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public int ResidentSend { get; set; }
        public ApplicationResident Resident { get; set; }

        [StringLength(100)]
        public string ResidentName { get; set; }

        [StringLength(255)]
        public string Title { get; set; }

        public string Content { get; set; }

        [StringLength(255)]
        public string ImageUrl { get; set; }

        [StringLength(50)]
        public string Type { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string Status { get; set; }
    }
}
