using ConvenienceStore.Models.Entities;
using ConvenienceStore.Models.ViewModels;

namespace ConvenienceStore.Web.ViewModels
{
    public class ChiTietSanPhamKhachViewModel
    {
        public SanPham? SanPham { get; set; }
        public List<SanPham> SanPhamLienQuan { get; set; } = new();
        public List<DanhGiaSanPham> DanhSachDanhGia { get; set; } = new();

        public bool CoTheDanhGia { get; set; }

        public DanhGiaSanPhamViewModel FormDanhGia { get; set; } = new();

        public ThongKeDanhGiaViewModel ThongKeDanhGia { get; set; } = new();
    }
}