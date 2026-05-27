using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.DataAccess;
using ConvenienceStore.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDichVuDanhMuc _dichVuDanhMuc;
        private readonly IDichVuSanPham _dichVuSanPham;
        private readonly ApplicationDbContext _context;

        public HomeController(
            IDichVuDanhMuc dichVuDanhMuc,
            IDichVuSanPham dichVuSanPham,
            ApplicationDbContext context)
        {
            _dichVuDanhMuc = dichVuDanhMuc;
            _dichVuSanPham = dichVuSanPham;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var danhMucs = await _dichVuDanhMuc.LayTatCaAsync();
            var flashSale = await _dichVuSanPham.LaySanPhamFlashSaleAsync(8);
            var comboFlashSale = await _context.Combos
                .Include(x => x.ComboSanPhams)
                    .ThenInclude(x => x.SanPham)
                .Where(x => x.TrangThai && x.ComboSanPhams.Count >= 2)
                .OrderByDescending(x => x.Id)
                .Take(8)
                .ToListAsync();
            var sanPhamMoi = await _dichVuSanPham.LaySanPhamMoiAsync(8);

            var model = new TrangChuViewModel
            {
                DanhSachDanhMuc = danhMucs.Where(x => x.TrangThai).ToList(),
                DanhSachFlashSale = flashSale.ToList(),
                DanhSachComboFlashSale = comboFlashSale,
                DanhSachSanPhamMoi = sanPhamMoi.ToList()
            };

            return View(model);
        }
    }
}
