namespace ConvenienceStore.Models.ViewModels
{
    public class ThongKeBaoCaoViewModel
    {
        public List<string> NhanDoanhThuTheoNgay { get; set; } = new();
        public List<decimal> DuLieuDoanhThuTheoNgay { get; set; } = new();

        public List<string> NhanDoanhThuTheoTuan { get; set; } = new();
        public List<decimal> DuLieuDoanhThuTheoTuan { get; set; } = new();

        public List<string> NhanDoanhThuTheoThang { get; set; } = new();
        public List<decimal> DuLieuDoanhThuTheoThang { get; set; } = new();

        public List<string> NhanDoanhThuTheoQuy { get; set; } = new();
        public List<decimal> DuLieuDoanhThuTheoQuy { get; set; } = new();

        public List<string> NhanDoanhThuTheoNam { get; set; } = new();
        public List<decimal> DuLieuDoanhThuTheoNam { get; set; } = new();

        public List<string> NhanSanPhamBanChay { get; set; } = new();
        public List<int> DuLieuSanPhamBanChay { get; set; } = new();

        public List<string> NhanSanPhamBanCham { get; set; } = new();
        public List<int> DuLieuSanPhamBanCham { get; set; } = new();

        public decimal TongDoanhThu { get; set; }
        public int TongDonDaGiao { get; set; }
    }
}