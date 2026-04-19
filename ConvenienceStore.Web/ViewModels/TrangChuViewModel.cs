using ConvenienceStore.Models.Entities;

namespace ConvenienceStore.Web.ViewModels
{
    public class TrangChuViewModel
    {
        public List<DanhMuc> DanhSachDanhMuc { get; set; } = new();
        public List<SanPham> DanhSachFlashSale { get; set; } = new();
        public List<SanPham> DanhSachSanPhamMoi { get; set; } = new();
    }
}