// Models/Post.cs
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace QuanLyTruyenThong_TuVan.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(255, ErrorMessage = "Tiêu đề tối đa 255 ký tự")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; }

        [StringLength(255)]
        [ValidateNever]
        public string ImageUrl { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Người gửi")]
        public string SenderId { get; set; }

        [ValidateNever]
        public ApplicationResident Sender { get; set; }

        [Required(ErrorMessage = "Chủ đề không được để trống")]
        [Display(Name = "Chủ đề bài viết")]
        public PostTopic Topic { get; set; }
    }

    public enum PostTopic
    {
        Technology,
        Education,
        Lifestyle,
        News,
        Opinion,
        Others
    }
}
