using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ConvenienceStore.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDichVuDanhMuc _dichVuDanhMuc;
        private readonly IDichVuSanPham _dichVuSanPham;

        public HomeController(IDichVuDanhMuc dichVuDanhMuc, IDichVuSanPham dichVuSanPham)
        {
            _dichVuDanhMuc = dichVuDanhMuc;
            _dichVuSanPham = dichVuSanPham;
        }

        public async Task<IActionResult> Index()
        {
            var danhMucs = await _dichVuDanhMuc.LayTatCaAsync();
            var flashSale = await _dichVuSanPham.LaySanPhamFlashSaleAsync(8);
            var sanPhamMoi = await _dichVuSanPham.LaySanPhamMoiAsync(8);

            var model = new TrangChuViewModel
            {
                DanhSachDanhMuc = danhMucs.Where(x => x.TrangThai).ToList(),
                DanhSachFlashSale = flashSale.ToList(),
                DanhSachSanPhamMoi = sanPhamMoi.ToList()
            };

            return View(model);
        }
    }
}