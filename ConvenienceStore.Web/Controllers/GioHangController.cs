using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.Models.Entities;
using ConvenienceStore.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace ConvenienceStore.Web.Controllers
{
    public class GioHangController : Controller
    {
        private readonly IDichVuSanPham _dichVuSanPham;
        private readonly IDichVuDonHang _dichVuDonHang;
        private const string TenSessionGioHang = "GIO_HANG";

        public GioHangController(IDichVuSanPham dichVuSanPham, IDichVuDonHang dichVuDonHang)
        {
            _dichVuSanPham = dichVuSanPham;
            _dichVuDonHang = dichVuDonHang;
        }

        public IActionResult Index()
        {
            var gioHang = LayGioHang();
            return View(gioHang);
        }

        [HttpPost]
        public async Task<IActionResult> ThemVaoGio(int sanPhamId, int soLuong = 1)
        {
            var sanPham = await _dichVuSanPham.LayTheoIdAsync(sanPhamId);
            if (sanPham == null)
                return NotFound();

            var gioHang = LayGioHang();
            var itemDaCo = gioHang.FirstOrDefault(x => x.SanPhamId == sanPhamId);

            if (itemDaCo == null)
            {
                gioHang.Add(new GioHangItemViewModel
                {
                    SanPhamId = sanPham.Id,
                    TenSanPham = sanPham.TenSanPham,
                    HinhAnh = sanPham.HinhAnh,
                    Gia = sanPham.Gia,
                    SoLuong = Math.Min(soLuong, sanPham.SoLuongTon),
                    SoLuongTon = sanPham.SoLuongTon
                });
            }
            else
            {
                itemDaCo.SoLuong += soLuong;
                if (itemDaCo.SoLuong > sanPham.SoLuongTon)
                    itemDaCo.SoLuong = sanPham.SoLuongTon;
            }

            LuuGioHang(gioHang);
            TempData["ThanhCong"] = "Đã thêm sản phẩm vào giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult CapNhatSoLuong(int sanPhamId, int soLuong)
        {
            var gioHang = LayGioHang();
            var item = gioHang.FirstOrDefault(x => x.SanPhamId == sanPhamId);

            if (item != null)
            {
                if (soLuong <= 0)
                {
                    gioHang.Remove(item);
                }
                else
                {
                    item.SoLuong = Math.Min(soLuong, item.SoLuongTon);
                }

                LuuGioHang(gioHang);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult XoaKhoiGio(int sanPhamId)
        {
            var gioHang = LayGioHang();
            var item = gioHang.FirstOrDefault(x => x.SanPhamId == sanPhamId);

            if (item != null)
            {
                gioHang.Remove(item);
                LuuGioHang(gioHang);
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public IActionResult DatHangTuGio()
        {
            var gioHang = LayGioHang();

            if (!gioHang.Any())
            {
                TempData["Loi"] = "Giỏ hàng đang trống.";
                return RedirectToAction(nameof(Index));
            }

            var model = new DatHangTuGioViewModel
            {
                DanhSachSanPham = gioHang
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DatHangTuGio(DatHangTuGioViewModel model)
        {
            var gioHang = LayGioHang();

            if (!gioHang.Any())
            {
                TempData["Loi"] = "Giỏ hàng đang trống.";
                return RedirectToAction(nameof(Index));
            }

            model.DanhSachSanPham = gioHang;

            if (!ModelState.IsValid)
                return View(model);

            var nguoiDungId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(nguoiDungId))
                return Challenge();

            try
            {
                var danhSachSanPham = gioHang.Select(x => new SanPhamDatHang
                {
                    SanPhamId = x.SanPhamId,
                    SoLuong = x.SoLuong
                }).ToList();

                var donHangId = await _dichVuDonHang.TaoDonHangTuGioAsync(
                    nguoiDungId,
                    danhSachSanPham,
                    model.HoTenNguoiNhan,
                    model.SoDienThoai,
                    model.DiaChiNhanHang);

                XoaToanBoGioHang();

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

        private List<GioHangItemViewModel> LayGioHang()
        {
            var duLieu = HttpContext.Session.GetString(TenSessionGioHang);

            if (string.IsNullOrEmpty(duLieu))
                return new List<GioHangItemViewModel>();

            return JsonSerializer.Deserialize<List<GioHangItemViewModel>>(duLieu) ?? new List<GioHangItemViewModel>();
        }

        private void LuuGioHang(List<GioHangItemViewModel> gioHang)
        {
            var duLieu = JsonSerializer.Serialize(gioHang);
            HttpContext.Session.SetString(TenSessionGioHang, duLieu);
        }

        private void XoaToanBoGioHang()
        {
            HttpContext.Session.Remove(TenSessionGioHang);
        }
    }
}