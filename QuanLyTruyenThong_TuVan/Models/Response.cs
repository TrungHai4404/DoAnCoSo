using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyTruyenThong_TuVan.Models
{
    public class Response
    {
        public int Id { get; set; }

        // Quan hệ tới Comment
        [Required]
        public int CommentId { get; set; }
        public Comment Comment { get; set; }

        // Tiêu đề của phản hồi (bắt buộc)
        [Required]
        [StringLength(255)]
        public string TitleComment { get; set; }

        // Nội dung phản hồi (bắt buộc)
        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // FK về Admin (ApplicationResident) gửi phản hồi
        [Required]
        public string ResidentId { get; set; }
        public ApplicationResident Resident { get; set; }
    }
}
