using System.ComponentModel.DataAnnotations;

namespace ConvenienceStore.Web.ViewModels
{
    public class KhuyenMaiViewModel
    {
        public int Id { get; set; }

        public string TenSanPham { get; set; } = string.Empty;

        public decimal GiaGoc { get; set; }

        public string? HinhAnh { get; set; }

        [Range(0, 100, ErrorMessage = "Phần trăm giảm phải từ 0 đến 100")]
        public int PhanTramGiam { get; set; }

        public DateTime? NgayBatDauKhuyenMai { get; set; }

        public DateTime? NgayKetThucKhuyenMai { get; set; }

        public bool DangKhuyenMai { get; set; }

        public decimal GiaSauGiam { get; set; }
    }
}