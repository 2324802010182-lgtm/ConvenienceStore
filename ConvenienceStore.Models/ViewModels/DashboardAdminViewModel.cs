namespace ConvenienceStore.Models.ViewModels
{
    public class DashboardAdminViewModel
    {
        public int TongSanPham { get; set; }
        public int TongDanhMuc { get; set; }
        public int TongDonHang { get; set; }
        public int TongNguoiDung { get; set; }

        public decimal TongDoanhThu { get; set; }
        public int DonChoXacNhan { get; set; }
        public int SanPhamSapHetHang { get; set; }

        public List<string> HoatDongGanDay { get; set; } = new();
    }
}