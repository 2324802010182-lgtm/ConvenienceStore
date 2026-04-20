using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.Models.Entities;
using ConvenienceStore.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConvenienceStore.Web.Controllers
{
    public class SanPhamKhachController : Controller
    {
        private readonly IDichVuSanPham _dichVuSanPham;
        private readonly IDichVuDanhGiaSanPham _dichVuDanhGiaSanPham;

        public SanPhamKhachController(
            IDichVuSanPham dichVuSanPham,
            IDichVuDanhGiaSanPham dichVuDanhGiaSanPham)
        {
            _dichVuSanPham = dichVuSanPham;
            _dichVuDanhGiaSanPham = dichVuDanhGiaSanPham;
        }

        public async Task<IActionResult> Index()
        {
            var danhSachSanPham = await _dichVuSanPham.LaySanPhamDangBanAsync();
            return View(danhSachSanPham);
        }

        public async Task<IActionResult> TheoDanhMuc(int id)
        {
            var danhSachSanPham = await _dichVuSanPham.LaySanPhamTheoDanhMucAsync(id);
            return View("Index", danhSachSanPham);
        }

        public async Task<IActionResult> ChiTiet(int id)
        {
            var sanPham = await _dichVuSanPham.LayTheoIdKemDanhMucAsync(id);
            if (sanPham == null)
                return NotFound();

            var tatCaSanPham = await _dichVuSanPham.LaySanPhamDangBanAsync();
            var danhSachDanhGia = await _dichVuDanhGiaSanPham.LayTheoSanPhamAsync(id);
            var thongKeDanhGia = await _dichVuDanhGiaSanPham.LayThongKeDanhGiaAsync(id);

            var sanPhamLienQuan = tatCaSanPham
                .Where(x => x.Id != sanPham.Id && x.DanhMucId == sanPham.DanhMucId)
                .Take(4)
                .ToList();

            bool coTheDanhGia = false;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var nguoiDungId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrEmpty(nguoiDungId))
                {
                    var daMua = await _dichVuDanhGiaSanPham.KiemTraNguoiDungDaMuaSanPhamAsync(nguoiDungId, id);
                    var daDanhGia = await _dichVuDanhGiaSanPham.KiemTraDaDanhGiaAsync(nguoiDungId, id);

                    coTheDanhGia = daMua && !daDanhGia;
                }
            }

            var model = new ChiTietSanPhamKhachViewModel
            {
                SanPham = sanPham,
                SanPhamLienQuan = sanPhamLienQuan,
                DanhSachDanhGia = danhSachDanhGia,
                ThongKeDanhGia = thongKeDanhGia,
                CoTheDanhGia = coTheDanhGia,
                FormDanhGia = new DanhGiaSanPhamViewModel
                {
                    SanPhamId = id
                }
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> GuiDanhGia(DanhGiaSanPhamViewModel model)
        {
            var nguoiDungId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(nguoiDungId))
                return Challenge();

            var daMua = await _dichVuDanhGiaSanPham.KiemTraNguoiDungDaMuaSanPhamAsync(nguoiDungId, model.SanPhamId);
            var daDanhGia = await _dichVuDanhGiaSanPham.KiemTraDaDanhGiaAsync(nguoiDungId, model.SanPhamId);

            if (!daMua || daDanhGia)
            {
                TempData["Loi"] = "Bạn chỉ có thể đánh giá khi đã mua và chưa đánh giá sản phẩm này.";
                return RedirectToAction(nameof(ChiTiet), new { id = model.SanPhamId });
            }

            if (!ModelState.IsValid)
            {
                TempData["Loi"] = "Dữ liệu đánh giá không hợp lệ.";
                return RedirectToAction(nameof(ChiTiet), new { id = model.SanPhamId });
            }

            string? duongDanAnh = null;

            if (model.TepHinhAnh != null && model.TepHinhAnh.Length > 0)
            {
                var thuMucLuuAnh = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "danhgia");

                if (!Directory.Exists(thuMucLuuAnh))
                {
                    Directory.CreateDirectory(thuMucLuuAnh);
                }

                var tenFile = Guid.NewGuid().ToString() + Path.GetExtension(model.TepHinhAnh.FileName);
                var duongDanDayDu = Path.Combine(thuMucLuuAnh, tenFile);

                using (var stream = new FileStream(duongDanDayDu, FileMode.Create))
                {
                    await model.TepHinhAnh.CopyToAsync(stream);
                }

                duongDanAnh = "/images/danhgia/" + tenFile;
            }

            var danhGia = new DanhGiaSanPham
            {
                SanPhamId = model.SanPhamId,
                NguoiDungId = nguoiDungId,
                SoSao = model.SoSao,
                BinhLuan = model.BinhLuan,
                HinhAnhDanhGia = duongDanAnh,
                NgayDanhGia = DateTime.Now,
                BiAn = false
            };

            await _dichVuDanhGiaSanPham.ThemDanhGiaAsync(danhGia);

            TempData["ThanhCong"] = "Đánh giá sản phẩm thành công.";
            return RedirectToAction(nameof(ChiTiet), new { id = model.SanPhamId });
        }

        public async Task<IActionResult> TimKiem(string tuKhoa)
        {
            var ketQua = await _dichVuSanPham.TimKiemSanPhamAsync(tuKhoa);
            ViewBag.TuKhoa = tuKhoa;
            return View(ketQua);
        }
    }
}