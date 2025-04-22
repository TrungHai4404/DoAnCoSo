using System.ComponentModel.DataAnnotations;

namespace QuanLyTruyenThong_TuVan.Models
{
    public class ForumPost
    {
        public int Id { get; set; }

        public int TopicId { get; set; }
        public Forum Forum { get; set; }

        public int ResidentId { get; set; }
        public ApplicationResident Resident { get; set; }

        [StringLength(50)]
        public string TypeTopic { get; set; }

        public string Content { get; set; }

        [StringLength(255)]
        public string ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsApproved { get; set; } = false;
    }
}
