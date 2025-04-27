using System.ComponentModel.DataAnnotations;

namespace QuanLyTruyenThong_TuVan.Models
{
    public class Comment
    {
        public int Id { get; set; }

        // Foreign key đến ApplicationResident (có thể cần thêm nếu bạn muốn thiết lập FK)
        public string ResidentId { get; set; }
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

        // Quan hệ 1-n với Response
        public ICollection<Response> Responses { get; set; }
    }

}
