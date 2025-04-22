namespace QuanLyTruyenThong_TuVan.Models
{
    public class Apartment
    {
        public int Id { get; set; }           // ID căn hộ (Khóa chính)
        public string ApartmentCode { get; set; }  // Mã căn hộ
        public string Floor { get; set; }     // Tầng của căn hộ
        public int RoomNumber { get; set; }   // Số phòng trong căn hộ
        public double Area { get; set; }      // Diện tích căn hộ
        public string Address { get; set; }   // Địa chỉ cụ thể của căn hộ

        // Khóa ngoại liên kết với User (Resident) nếu mỗi người dùng có thể có một căn hộ
        public int ResidentId { get; set; }
        public ApplicationResident Resident { get; set; }  // Liên kết đến ApplicationUser (Resident)

        // Các thuộc tính khác nếu cần
    }
}
