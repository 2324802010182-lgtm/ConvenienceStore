using ConvenienceStore.Models.Entities;

namespace ConvenienceStore.Web.ViewModels
{
    public class ChiTietSanPhamKhachViewModel
    {
        public SanPham? SanPham { get; set; }
        public List<SanPham> SanPhamLienQuan { get; set; } = new();
    }
}