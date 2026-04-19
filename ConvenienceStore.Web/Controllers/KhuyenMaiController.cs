using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.Models.HangSo;
using ConvenienceStore.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConvenienceStore.Web.Controllers
{
    [Authorize(Roles = VaiTro.Admin)]
    public class KhuyenMaiController : Controller
    {
        private readonly IDichVuSanPham _dichVuSanPham;

        public KhuyenMaiController(IDichVuSanPham dichVuSanPham)
        {
            _dichVuSanPham = dichVuSanPham;
        }

        public async Task<IActionResult> Index()
        {
            var danhSachSanPham = await _dichVuSanPham.LayTatCaKemDanhMucAsync();

            var model = danhSachSanPham.Select(sp => new KhuyenMaiViewModel
            {
                Id = sp.Id,
                TenSanPham = sp.TenSanPham,
                GiaGoc = sp.Gia,
                HinhAnh = sp.HinhAnh,
                PhanTramGiam = sp.PhanTramGiam,
                NgayBatDauKhuyenMai = sp.NgayBatDauKhuyenMai,
                NgayKetThucKhuyenMai = sp.NgayKetThucKhuyenMai,
                DangKhuyenMai = sp.DangKhuyenMai,
                GiaSauGiam = sp.GiaSauGiam
            }).ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ChinhSua(int id)
        {
            var sanPham = await _dichVuSanPham.LayTheoIdAsync(id);
            if (sanPham == null)
                return NotFound();

            var model = new KhuyenMaiViewModel
            {
                Id = sanPham.Id,
                TenSanPham = sanPham.TenSanPham,
                GiaGoc = sanPham.Gia,
                HinhAnh = sanPham.HinhAnh,
                PhanTramGiam = sanPham.PhanTramGiam,
                NgayBatDauKhuyenMai = sanPham.NgayBatDauKhuyenMai,
                NgayKetThucKhuyenMai = sanPham.NgayKetThucKhuyenMai,
                DangKhuyenMai = sanPham.DangKhuyenMai,
                GiaSauGiam = sanPham.GiaSauGiam
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChinhSua(KhuyenMaiViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var sanPham = await _dichVuSanPham.LayTheoIdAsync(model.Id);
            if (sanPham == null)
                return NotFound();

            sanPham.PhanTramGiam = model.PhanTramGiam;
            sanPham.NgayBatDauKhuyenMai = model.NgayBatDauKhuyenMai;
            sanPham.NgayKetThucKhuyenMai = model.NgayKetThucKhuyenMai;

            await _dichVuSanPham.CapNhatAsync(sanPham);

            TempData["ThanhCong"] = "Cập nhật khuyến mãi thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> HuyKhuyenMai(int id)
        {
            var sanPham = await _dichVuSanPham.LayTheoIdAsync(id);
            if (sanPham == null)
                return NotFound();

            sanPham.PhanTramGiam = 0;
            sanPham.NgayBatDauKhuyenMai = null;
            sanPham.NgayKetThucKhuyenMai = null;

            await _dichVuSanPham.CapNhatAsync(sanPham);

            TempData["ThanhCong"] = "Đã hủy khuyến mãi.";
            return RedirectToAction(nameof(Index));
        }
    }
}