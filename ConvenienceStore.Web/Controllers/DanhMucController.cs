using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.Models.Entities;
using ConvenienceStore.Models.HangSo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConvenienceStore.Web.Controllers
{
    [Authorize(Roles = VaiTro.Admin)]
    public class DanhMucController : Controller
    {
        private readonly IDichVuDanhMuc _dichVuDanhMuc;

        public DanhMucController(IDichVuDanhMuc dichVuDanhMuc)
        {
            _dichVuDanhMuc = dichVuDanhMuc;
        }

        public async Task<IActionResult> Index()
        {
            var danhSachDanhMuc = await _dichVuDanhMuc.LayTatCaAsync();
            return View(danhSachDanhMuc);
        }

        [HttpGet]
        public IActionResult ThemMoi()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ThemMoi(DanhMuc danhMuc)
        {
            if (!ModelState.IsValid)
                return View(danhMuc);

            await _dichVuDanhMuc.ThemAsync(danhMuc);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ChinhSua(int id)
        {
            var danhMuc = await _dichVuDanhMuc.LayTheoIdAsync(id);
            if (danhMuc == null)
                return NotFound();

            return View(danhMuc);
        }

        [HttpPost]
        public async Task<IActionResult> ChinhSua(DanhMuc danhMuc)
        {
            if (!ModelState.IsValid)
                return View(danhMuc);

            await _dichVuDanhMuc.CapNhatAsync(danhMuc);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Xoa(int id)
        {
            var danhMuc = await _dichVuDanhMuc.LayTheoIdAsync(id);
            if (danhMuc == null)
                return NotFound();

            return View(danhMuc);
        }

        [HttpPost, ActionName("Xoa")]
        public async Task<IActionResult> XacNhanXoa(int id)
        {
            await _dichVuDanhMuc.XoaAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}