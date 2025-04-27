using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace QuanLyTruyenThong_TuVan.Models {
    public class New {
        public int Id { get; set; }

        [StringLength(255)]
        public string Title { get; set; }

        public string Notes { get; set; }

        public string Content { get; set; }

        [StringLength(255)]
        [ValidateNever]

        public string? ImagesUrl { get; set; }  // Không cần ValidateNever ở đây nếu bạn muốn xử lý nó đúng cách

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? SendAt { get; set; }

        public string SenderId { get; set; }
        [ValidateNever]

        public ApplicationResident Sender { get; set; }

        [StringLength(50)]
        public string Category { get; set; }

        [StringLength(50)]
        public string Status { get; set; }
    }
}
