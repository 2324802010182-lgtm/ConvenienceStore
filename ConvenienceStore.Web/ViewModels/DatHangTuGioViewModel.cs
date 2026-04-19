using System.ComponentModel.DataAnnotations;

namespace ConvenienceStore.Web.ViewModels
{
    public class DatHangTuGioViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
        public string HoTenNguoiNhan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
        public string DiaChiNhanHang { get; set; } = string.Empty;

        public List<GioHangItemViewModel> DanhSachSanPham { get; set; } = new();

        public decimal TongTien => DanhSachSanPham.Sum(x => x.ThanhTien);
    }
}