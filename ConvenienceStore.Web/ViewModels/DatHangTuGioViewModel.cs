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

        public int DiemHienCo { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số điểm muốn dùng không hợp lệ")]
        public int DiemMuonDoi { get; set; }

        public decimal TienGiamTuDiem
        {
            get
            {
                var tienGiam = DiemMuonDoi * 1000m;
                return tienGiam > TongTien ? TongTien : tienGiam;
            }
        }

        public decimal TongTienSauGiam
        {
            get
            {
                var tongSauGiam = TongTien - TienGiamTuDiem;
                return tongSauGiam < 0 ? 0 : tongSauGiam;
            }
        }
    }
}