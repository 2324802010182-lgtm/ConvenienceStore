using System.ComponentModel.DataAnnotations;

namespace ConvenienceStore.Web.ViewModels
{
    public class DatHangViewModel
    {
        public int SanPhamId { get; set; }

        public string TenSanPham { get; set; } = string.Empty;

        public decimal Gia { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
        public string HoTenNguoiNhan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
        public string DiaChiNhanHang { get; set; } = string.Empty;

        [Range(1, 100, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuong { get; set; } = 1;
    }
}