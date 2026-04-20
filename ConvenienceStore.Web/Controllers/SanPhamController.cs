using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.Models.Entities;
using ConvenienceStore.Models.HangSo;
using ConvenienceStore.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace ConvenienceStore.Web.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly IDichVuSanPham _dichVuSanPham;
        private readonly IDichVuDanhMuc _dichVuDanhMuc;
        private readonly IDichVuDonHang _dichVuDonHang;

        public SanPhamController(
            IDichVuSanPham dichVuSanPham,
            IDichVuDanhMuc dichVuDanhMuc,
            IDichVuDonHang dichVuDonHang)
        {
            _dichVuSanPham = dichVuSanPham;
            _dichVuDanhMuc = dichVuDanhMuc;
            _dichVuDonHang = dichVuDonHang;
        }

        [Authorize(Roles = VaiTro.Admin + "," + VaiTro.NhanVien)]
        public async Task<IActionResult> Index()
        {
            var danhSachSanPham = await _dichVuSanPham.LayTatCaKemDanhMucAsync();
            return View(danhSachSanPham);
        }

        [Authorize(Roles = VaiTro.Admin + "," + VaiTro.NhanVien)]
        [HttpGet]
        public async Task<IActionResult> ThemMoi()
        {
            var model = new SanPhamViewModel
            {
                DanhSachDanhMuc = await LayDanhSachDanhMucAsync()
            };

            return View(model);
        }

        [Authorize(Roles = VaiTro.Admin + "," + VaiTro.NhanVien)]
        [HttpPost]
        public async Task<IActionResult> ThemMoi(SanPhamViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.DanhSachDanhMuc = await LayDanhSachDanhMucAsync();
                return View(model);
            }

            var sanPham = new SanPham
            {
                TenSanPham = model.TenSanPham,
                MoTa = model.MoTa,
                Gia = model.Gia,
                SoLuongTon = model.SoLuongTon,
                HinhAnh = model.HinhAnh,
                TrangThai = model.TrangThai,
                DanhMucId = model.DanhMucId
            };

            await _dichVuSanPham.ThemAsync(sanPham);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = VaiTro.Admin + "," + VaiTro.NhanVien)]
        [HttpGet]
        public async Task<IActionResult> ChinhSua(int id)
        {
            var sanPham = await _dichVuSanPham.LayTheoIdAsync(id);
            if (sanPham == null)
                return NotFound();

            var model = new SanPhamViewModel
            {
                Id = sanPham.Id,
                TenSanPham = sanPham.TenSanPham,
                MoTa = sanPham.MoTa,
                Gia = sanPham.Gia,
                SoLuongTon = sanPham.SoLuongTon,
                HinhAnh = sanPham.HinhAnh,
                TrangThai = sanPham.TrangThai,
                DanhMucId = sanPham.DanhMucId,
                DanhSachDanhMuc = await LayDanhSachDanhMucAsync()
            };

            return View(model);
        }

        [Authorize(Roles = VaiTro.Admin + "," + VaiTro.NhanVien)]
        [HttpPost]
        public async Task<IActionResult> ChinhSua(SanPhamViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.DanhSachDanhMuc = await LayDanhSachDanhMucAsync();
                return View(model);
            }

            var sanPhamCu = await _dichVuSanPham.LayTheoIdAsync(model.Id);
            if (sanPhamCu == null)
                return NotFound();

            var sanPham = new SanPham
            {
                Id = model.Id,
                TenSanPham = model.TenSanPham,
                MoTa = model.MoTa,
                Gia = model.Gia,
                SoLuongTon = model.SoLuongTon,
                HinhAnh = model.HinhAnh,
                TrangThai = model.TrangThai,
                DanhMucId = model.DanhMucId,

                // Giữ nguyên dữ liệu khuyến mãi cũ
                PhanTramGiam = sanPhamCu.PhanTramGiam,
                NgayBatDauKhuyenMai = sanPhamCu.NgayBatDauKhuyenMai,
                NgayKetThucKhuyenMai = sanPhamCu.NgayKetThucKhuyenMai
            };

            await _dichVuSanPham.CapNhatAsync(sanPham);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = VaiTro.Admin + "," + VaiTro.NhanVien)]
        [HttpGet]
        public async Task<IActionResult> Xoa(int id)
        {
            var sanPham = await _dichVuSanPham.LayTheoIdAsync(id);
            if (sanPham == null)
                return NotFound();

            return View(sanPham);
        }

        [Authorize(Roles = VaiTro.Admin + "," + VaiTro.NhanVien)]
        [HttpPost, ActionName("Xoa")]
        public async Task<IActionResult> XacNhanXoa(int id)
        {
            await _dichVuSanPham.XoaAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DatHang(int id)
        {
            var sanPham = await _dichVuSanPham.LayTheoIdAsync(id);
            if (sanPham == null)
                return NotFound();

            var giaApDung = sanPham.DangKhuyenMai ? sanPham.GiaSauGiam : sanPham.Gia;

            var model = new DatHangViewModel
            {
                SanPhamId = sanPham.Id,
                TenSanPham = sanPham.TenSanPham,
                Gia = giaApDung,
                SoLuong = 1
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DatHang(DatHangViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var nguoiDungId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(nguoiDungId))
                return Challenge();

            try
            {
                var donHangId = await _dichVuDonHang.TaoDonHangAsync(
                    nguoiDungId,
                    model.SanPhamId,
                    model.SoLuong,
                    model.HoTenNguoiNhan,
                    model.SoDienThoai,
                    model.DiaChiNhanHang);

                return RedirectToAction(nameof(DatHangThanhCong), new { id = donHangId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult DatHangThanhCong(int id)
        {
            ViewBag.DonHangId = id;
            return View();
        }

        private async Task<IEnumerable<SelectListItem>> LayDanhSachDanhMucAsync()
        {
            var danhMucs = await _dichVuDanhMuc.LayTatCaAsync();

            return danhMucs
                .Where(dm => dm.TrangThai)
                .Select(dm => new SelectListItem
                {
                    Value = dm.Id.ToString(),
                    Text = dm.TenDanhMuc
                });
        }
    }
}