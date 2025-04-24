using System.ComponentModel.DataAnnotations;

namespace QuanLyTruyenThong_TuVan.Models
{
    public class Apartment
    {
        public int Id { get; set; }           // ID căn hộ (Khóa chính)
        [Required(ErrorMessage = "Vui lòng nhập mã căn hộ")]
        public string ApartmentCode { get; set; }  // Mã căn hộ
        public string Floor { get; set; }     // Tầng của căn hộ
        public int RoomNumber { get; set; }   // Số phòng trong căn hộ
        public double Area { get; set; }      // Diện tích căn hộ
        public string Address { get; set; }   // Địa chỉ cụ thể của căn hộ

        // Khóa ngoại 
        public ICollection<ApplicationResident>? Residents { get; set; }  // Liên kết đến ApplicationResident
    }
}
