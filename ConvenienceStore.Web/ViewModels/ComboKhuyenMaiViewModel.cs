using System.ComponentModel.DataAnnotations;

namespace ConvenienceStore.Web.ViewModels
{
    public class QuanLyKhuyenMaiViewModel
    {
        public List<ComboKhuyenMaiViewModel> DanhSachCombo { get; set; } = new();

        public List<KhuyenMaiViewModel> DanhSachSanPham { get; set; } = new();
    }

    public class ComboKhuyenMaiViewModel
    {
        public int Id { get; set; }

        public string TenCombo { get; set; } = string.Empty;

        public string DanhSachSanPham { get; set; } = string.Empty;

        public decimal GiaGoc { get; set; }

        public decimal GiaCombo { get; set; }

        public int PhanTramTietKiem { get; set; }

        public bool TrangThai { get; set; }
    }

    public class ComboKhuyenMaiFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên combo không được để trống")]
        [StringLength(200, ErrorMessage = "Tên combo tối đa 200 ký tự")]
        public string TenCombo { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự")]
        public string? MoTa { get; set; }

        [Range(1000, double.MaxValue, ErrorMessage = "Giá combo phải lớn hơn 0")]
        public decimal GiaCombo { get; set; }

        public bool TrangThai { get; set; } = true;

        public List<int> SanPhamIds { get; set; } = new();

        public List<ComboSanPhamLuaChonViewModel> DanhSachSanPham { get; set; } = new();
    }

    public class ComboSanPhamLuaChonViewModel
    {
        public int Id { get; set; }

        public string TenSanPham { get; set; } = string.Empty;

        public decimal Gia { get; set; }

        public string? HinhAnh { get; set; }

        public bool DuocChon { get; set; }
    }
}
