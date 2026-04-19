namespace ConvenienceStore.Web.ViewModels
{
    public class ChiTietDonHangAdminViewModel
    {
        public int Id { get; set; }
        public string HoTenNguoiNhan { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string DiaChiNhanHang { get; set; } = string.Empty;
        public string? EmailNguoiDat { get; set; }
        public decimal TongTien { get; set; }
        public DateTime NgayDatHang { get; set; }
        public string TrangThai { get; set; } = string.Empty;

        public List<ChiTietSanPhamTrongDonViewModel> DanhSachSanPham { get; set; } = new();
    }

    public class ChiTietSanPhamTrongDonViewModel
    {
        public string TenSanPham { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public string? HinhAnh { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
    }
}