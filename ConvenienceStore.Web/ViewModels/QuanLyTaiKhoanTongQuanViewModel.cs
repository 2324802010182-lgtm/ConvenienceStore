namespace ConvenienceStore.Web.ViewModels
{
    public class QuanLyTaiKhoanTongQuanViewModel
    {
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public int TongDonHang { get; set; }
        public int DonDangXuLy { get; set; }
        public int DonDangGiao { get; set; }
        public int DonHoanThanh { get; set; }

        public List<DonHangTomTatViewModel> DonHangGanDay { get; set; } = new();
    }

    public class DonHangTomTatViewModel
    {
        public int Id { get; set; }
        public DateTime NgayDat { get; set; }
        public int SoLuongSanPham { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string CssTrangThai { get; set; } = string.Empty;
    }
}