using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyTruyenThong_TuVan.Models
{
    public class InboxMessage
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationResident User { get; set; }

        [Required, StringLength(255)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;
    }
}
