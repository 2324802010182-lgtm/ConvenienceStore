using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.Models.HangSo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConvenienceStore.Web.Controllers
{
    [Authorize(Roles = VaiTro.Admin + "," + VaiTro.NhanVien)]
    public class DanhGiaController : Controller
    {
        private readonly IDichVuDanhGiaSanPham _dichVuDanhGiaSanPham;

        public DanhGiaController(IDichVuDanhGiaSanPham dichVuDanhGiaSanPham)
        {
            _dichVuDanhGiaSanPham = dichVuDanhGiaSanPham;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _dichVuDanhGiaSanPham.LayTatCaAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AnDanhGia(int id)
        {
            var danhGia = await _dichVuDanhGiaSanPham.LayTheoIdAsync(id);
            if (danhGia == null) return NotFound();

            danhGia.BiAn = true;
            await _dichVuDanhGiaSanPham.CapNhatAsync(danhGia);

            TempData["ThanhCong"] = "Đã ẩn đánh giá.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> HienDanhGia(int id)
        {
            var danhGia = await _dichVuDanhGiaSanPham.LayTheoIdAsync(id);
            if (danhGia == null) return NotFound();

            danhGia.BiAn = false;
            await _dichVuDanhGiaSanPham.CapNhatAsync(danhGia);

            TempData["ThanhCong"] = "Đã hiển thị lại đánh giá.";
            return RedirectToAction(nameof(Index));
        }
    }
}