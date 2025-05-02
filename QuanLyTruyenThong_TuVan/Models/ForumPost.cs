// Models/ForumPost.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyTruyenThong_TuVan.Models
{
    public class ForumPost
    {
        public int Id { get; set; }

        [Required]
        public int TopicId { get; set; }

        public Forum Forum { get; set; } = null!;

        [Required]
        public string ResidentId { get; set; } = null!;

        public ApplicationResident Resident { get; set; } = null!;

        [Required]
        public TypeTopic TypeTopic { get; set; } 

        [Required]
        public string Content { get; set; } = null!;

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsApproved { get; set; } = false;
    }
}
