using System.ComponentModel.DataAnnotations;

namespace QuanLyTruyenThong_TuVan.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề bắt buộc.")]
        [StringLength(100)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Nội dung bắt buộc.")]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;


        [Required(ErrorMessage = "Họ tên người gửi là bắt buộc.")]
        public string SenderName { get; set; } // Trường họ tên người gửi
    }
}
