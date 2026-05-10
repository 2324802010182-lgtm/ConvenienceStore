namespace ConvenienceStore.Web.ViewModels
{
    public class DonHangAdminViewModel
    {
        public int Id { get; set; }

        public string HoTenNguoiNhan { get; set; } = string.Empty;

        public string? EmailNguoiDat { get; set; }

        public string SoDienThoai { get; set; } = string.Empty;

        public decimal TongTien { get; set; }

        public int DiemDaSuDung { get; set; }

        public decimal TienGiamTuDiem { get; set; }

        public decimal TongTienSauGiam { get; set; }

        public DateTime NgayDatHang { get; set; }

        public string TrangThai { get; set; } = string.Empty;
    }
}