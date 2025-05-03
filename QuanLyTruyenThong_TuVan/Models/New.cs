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
        public string ImagesUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? SendAt { get; set; }

        public string SenderId { get; set; }
        [ValidateNever]
        public ApplicationResident Sender { get; set; }
        public Category DanhMuc { get; set; }
        public Status TrangThai { get; set; }
    }
    public enum Category {
        Thông_Báo,
        Cháy_Nổ,
        Sự_Kiện,
        Khuyến_Mãi,
        Nâng_Cấp,
        Khoa_Học
    }
    public enum Status {
        Hoạt_động,
        Nháp,
        Kết_thúc
    }
}
