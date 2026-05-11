using ConvenienceStore.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ConvenienceStore.Models.Entities
{
    public class HoiThoaiChat
    {
        public int Id { get; set; }

        [Required]
        public string KhachHangId { get; set; } = string.Empty;
        public NguoiDung? KhachHang { get; set; }

        public string? NhanVienPhuTrachId { get; set; }
        public NguoiDung? NhanVienPhuTrach { get; set; }

        [StringLength(200)]
        public string TieuDe { get; set; } = "Hỗ trợ trực tuyến";

        public TrangThaiHoiThoaiChat TrangThai { get; set; } = TrangThaiHoiThoaiChat.ChoHoTro;

        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime LanHoatDongCuoi { get; set; } = DateTime.Now;

        public bool KhachHangDaDoc { get; set; } = true;
        public bool QuanTriDaDoc { get; set; } = false;

        public ICollection<TinNhanChat> TinNhans { get; set; } = new List<TinNhanChat>();
    }
}