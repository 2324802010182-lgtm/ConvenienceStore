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
        private readonly IDichVuDiemTichLuy _dichVuDiemTichLuy;

        private const string TenSessionGioHang = "GIO_HANG";

        public GioHangController(
            IDichVuSanPham dichVuSanPham,
            IDichVuDonHang dichVuDonHang,
            IDichVuDiemTichLuy dichVuDiemTichLuy)
        {
            _dichVuSanPham = dichVuSanPham;
            _dichVuDonHang = dichVuDonHang;
            _dichVuDiemTichLuy = dichVuDiemTichLuy;
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
        public async Task<IActionResult> DatHangTuGio()
        {
            var gioHang = LayGioHang();

            if (!gioHang.Any())
            {
                TempData["Loi"] = "Giỏ hàng đang trống.";
                return RedirectToAction(nameof(Index));
            }

            var nguoiDungId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(nguoiDungId))
                return Challenge();

            var diemHienCo = await _dichVuDiemTichLuy.LayDiemHienCoAsync(nguoiDungId);

            var model = new DatHangTuGioViewModel
            {
                DanhSachSanPham = gioHang,
                DiemHienCo = diemHienCo,
                DiemMuonDoi = 0
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DatHangTuGio(DatHangTuGioViewModel model)
        {
            var gioHang = LayGioHang();

            if (!gioHang.Any())
            {
                TempData["Loi"] = "Giỏ hàng đang trống.";
                return RedirectToAction(nameof(Index));
            }

            model.DanhSachSanPham = gioHang;

            var nguoiDungId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(nguoiDungId))
                return Challenge();

            var diemHienCo = await _dichVuDiemTichLuy.LayDiemHienCoAsync(nguoiDungId);
            model.DiemHienCo = diemHienCo;

            var tongTien = gioHang.Sum(x => x.ThanhTien);

            var diemToiDaTheoTongTien = (int)Math.Floor(tongTien / 1000m);
            var diemToiDaDuocDung = Math.Min(diemHienCo, diemToiDaTheoTongTien);

            if (model.DiemMuonDoi < 0)
            {
                model.DiemMuonDoi = 0;
            }

            if (model.DiemMuonDoi > diemToiDaDuocDung)
            {
                model.DiemMuonDoi = diemToiDaDuocDung;
            }

            var tienGiamTuDiem = model.DiemMuonDoi * 1000m;

            if (tienGiamTuDiem > tongTien)
            {
                tienGiamTuDiem = tongTien;
            }

            var tongTienSauGiam = tongTien - tienGiamTuDiem;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

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
                    model.DiaChiNhanHang,
                    model.DiemMuonDoi,
                    tienGiamTuDiem);

                if (model.DiemMuonDoi > 0)
                {
                    await _dichVuDiemTichLuy.DoiDiemAsync(
                        nguoiDungId,
                        donHangId,
                        model.DiemMuonDoi);
                }

                // Không cộng điểm ở đây.
                // Điểm chỉ nên cộng khi admin/nhân viên chuyển đơn sang trạng thái Đã giao.

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

            return JsonSerializer.Deserialize<List<GioHangItemViewModel>>(duLieu)
                ?? new List<GioHangItemViewModel>();
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