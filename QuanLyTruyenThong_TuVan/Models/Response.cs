using System.ComponentModel.DataAnnotations;

namespace QuanLyTruyenThong_TuVan.Models
{
    public class Response
    {
        public int Id { get; set; }

        // Foreign key đến ApplicationResident
        public string ResidentId { get; set; }
        public ApplicationResident Resident { get; set; }

        // Foreign key đến Comment
        public int CommentId { get; set; }
        public Comment Comment { get; set; }

        [StringLength(255)]
        public string TitleComment { get; set; }

        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
