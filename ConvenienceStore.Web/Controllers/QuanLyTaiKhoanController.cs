using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.Models.Entities;
using ConvenienceStore.Models.Enums;
using ConvenienceStore.Web.Services;
using ConvenienceStore.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace ConvenienceStore.Web.Controllers
{
    [Authorize]
    public class QuanLyTaiKhoanController : Controller
    {
        private readonly UserManager<NguoiDung> _userManager;
        private readonly SignInManager<NguoiDung> _signInManager;
        private readonly IDichVuDonHang _dichVuDonHang;
        private readonly IDichVuEmail _dichVuEmail;

        public QuanLyTaiKhoanController(
            UserManager<NguoiDung> userManager,
            SignInManager<NguoiDung> signInManager,
            IDichVuDonHang dichVuDonHang,
            IDichVuEmail dichVuEmail)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dichVuDonHang = dichVuDonHang;
            _dichVuEmail = dichVuEmail;
        }

        public async Task<IActionResult> Index()
        {
            var nguoiDung = await LayNguoiDungHienTaiAsync();
            if (nguoiDung == null)
                return Challenge();

            var danhSachDonHang = await _dichVuDonHang.LayDonHangTheoNguoiDungAsync(nguoiDung.Id);

            var donHangGanDay = danhSachDonHang
                .OrderByDescending(x => x.NgayDatHang)
                .Take(5)
                .Select(x => new DonHangTomTatViewModel
                {
                    Id = x.Id,
                    NgayDat = x.NgayDatHang,
                    SoLuongSanPham = x.ChiTietDonHangs?.Sum(ct => ct.SoLuong) ?? 0,
                    TongTien = x.TongTien,
                    TrangThai = LayTenTrangThai(x.TrangThai),
                    CssTrangThai = LayCssTrangThai(x.TrangThai)
                })
                .ToList();

            var model = new QuanLyTaiKhoanTongQuanViewModel
            {
                HoTen = nguoiDung.HoTen ?? string.Empty,
                Email = nguoiDung.Email ?? string.Empty,
                TongDonHang = danhSachDonHang.Count(),
                DonDangXuLy = danhSachDonHang.Count(x =>
                    x.TrangThai == TrangThaiDonHang.ChoXacNhan ||
                    x.TrangThai == TrangThaiDonHang.DaXacNhan ||
                    x.TrangThai == TrangThaiDonHang.DangChuanBi),
                DonDangGiao = danhSachDonHang.Count(x => x.TrangThai == TrangThaiDonHang.DangGiao),
                DonHoanThanh = danhSachDonHang.Count(x => x.TrangThai == TrangThaiDonHang.DaGiao),
                DonHangGanDay = donHangGanDay
            };

            ViewBag.MenuHienTai = "TongQuan";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> HoSo()
        {
            var nguoiDung = await LayNguoiDungHienTaiAsync();
            if (nguoiDung == null)
                return Challenge();

            var model = new CapNhatHoSoViewModel
            {
                HoTen = nguoiDung.HoTen ?? string.Empty,
                Email = nguoiDung.Email ?? string.Empty,
                DiaChi = nguoiDung.DiaChi ?? string.Empty,
                SoDienThoai = nguoiDung.PhoneNumber ?? string.Empty
            };

            ViewBag.MenuHienTai = "HoSo";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HoSo(CapNhatHoSoViewModel model)
        {
            ViewBag.MenuHienTai = "HoSo";

            if (!ModelState.IsValid)
                return View(model);

            var nguoiDung = await LayNguoiDungHienTaiAsync();
            if (nguoiDung == null)
                return Challenge();

            nguoiDung.HoTen = model.HoTen;
            nguoiDung.DiaChi = model.DiaChi;
            nguoiDung.PhoneNumber = model.SoDienThoai;

            var ketQua = await _userManager.UpdateAsync(nguoiDung);

            if (!ketQua.Succeeded)
            {
                foreach (var loi in ketQua.Errors)
                    ModelState.AddModelError("", loi.Description);

                return View(model);
            }

            TempData["ThanhCong"] = "Cập nhật hồ sơ thành công.";
            return RedirectToAction(nameof(HoSo));
        }

        [HttpGet]
        public async Task<IActionResult> DonHang()
        {
            var nguoiDungId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(nguoiDungId))
                return Challenge();

            var danhSach = await _dichVuDonHang.LayDonHangTheoNguoiDungAsync(nguoiDungId);

            var model = danhSach
                .OrderByDescending(x => x.NgayDatHang)
                .Select(x => new DonHangAdminViewModel
                {
                    Id = x.Id,
                    HoTenNguoiNhan = x.HoTenNguoiNhan,
                    EmailNguoiDat = x.NguoiDung != null ? x.NguoiDung.Email : "",
                    SoDienThoai = x.SoDienThoai,
                    TongTien = x.TongTien,
                    NgayDatHang = x.NgayDatHang,
                    TrangThai = LayTenTrangThai(x.TrangThai)
                })
                .ToList();

            ViewBag.MenuHienTai = "DonHang";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ChiTietDonHang(int id)
        {
            var nguoiDungId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(nguoiDungId))
                return Challenge();

            var donHang = await _dichVuDonHang.LayChiTietAsync(id);
            if (donHang == null || donHang.NguoiDungId != nguoiDungId)
                return NotFound();

            var model = new ChiTietDonHangAdminViewModel
            {
                Id = donHang.Id,
                HoTenNguoiNhan = donHang.HoTenNguoiNhan,
                SoDienThoai = donHang.SoDienThoai,
                DiaChiNhanHang = donHang.DiaChiNhanHang,
                EmailNguoiDat = donHang.NguoiDung?.Email,
                TongTien = donHang.TongTien,
                NgayDatHang = donHang.NgayDatHang,
                TrangThai = LayTenTrangThai(donHang.TrangThai),
                DanhSachSanPham = donHang.ChiTietDonHangs?.Select(ct => new ChiTietSanPhamTrongDonViewModel
                {
                    TenSanPham = ct.SanPham != null ? ct.SanPham.TenSanPham : "",
                    HinhAnh = ct.SanPham != null ? ct.SanPham.HinhAnh : null,
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGia,
                    ThanhTien = ct.ThanhTien
                }).ToList() ?? new List<ChiTietSanPhamTrongDonViewModel>()
            };

            ViewBag.MenuHienTai = "DonHang";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DoiMatKhau()
        {
            var nguoiDung = await LayNguoiDungHienTaiAsync();
            if (nguoiDung == null)
                return Challenge();

            var model = new DoiMatKhauTaiKhoanViewModel
            {
                Email = nguoiDung.Email ?? string.Empty
            };

            ViewBag.MenuHienTai = "DoiMatKhau";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiLinkDoiMatKhau()
        {
            var nguoiDung = await LayNguoiDungHienTaiAsync();
            if (nguoiDung == null)
                return Challenge();

            var token = await _userManager.GeneratePasswordResetTokenAsync(nguoiDung);
            var tokenMaHoa = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var linkDatLaiMatKhau = Url.Action(
                "DatLaiMatKhau",
                "TaiKhoan",
                new { token = tokenMaHoa, email = nguoiDung.Email },
                Request.Scheme);

            var noiDung = $@"
                <h3>Đổi mật khẩu tài khoản</h3>
                <p>Bạn đã yêu cầu đổi mật khẩu cho tài khoản Convenience Store.</p>
                <p>Vui lòng bấm vào liên kết bên dưới để tiếp tục:</p>
                <p>
                    <a href='{HtmlEncoder.Default.Encode(linkDatLaiMatKhau!)}'>
                        Đổi mật khẩu ngay
                    </a>
                </p>
                <p>Nếu không bấm được, hãy copy link này vào trình duyệt:</p>
                <p>{HtmlEncoder.Default.Encode(linkDatLaiMatKhau!)}</p>";

            await _dichVuEmail.GuiEmailAsync(
                nguoiDung.Email!,
                "Xác nhận đổi mật khẩu",
                noiDung);

            TempData["ThanhCong"] = "Đã gửi liên kết đổi mật khẩu về email của bạn.";
            return RedirectToAction(nameof(DoiMatKhau));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangXuat()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private async Task<NguoiDung?> LayNguoiDungHienTaiAsync()
        {
            return await _userManager.GetUserAsync(User);
        }

        private string LayTenTrangThai(TrangThaiDonHang trangThai)
        {
            return trangThai switch
            {
                TrangThaiDonHang.ChoXacNhan => "Chờ xác nhận",
                TrangThaiDonHang.DaXacNhan => "Đã xác nhận",
                TrangThaiDonHang.DangChuanBi => "Đang chuẩn bị",
                TrangThaiDonHang.DangGiao => "Đang giao",
                TrangThaiDonHang.DaGiao => "Đã giao",
                TrangThaiDonHang.DaHuy => "Đã hủy",
                _ => "Không xác định"
            };
        }

        private string LayCssTrangThai(TrangThaiDonHang trangThai)
        {
            return trangThai switch
            {
                TrangThaiDonHang.ChoXacNhan => "warning",
                TrangThaiDonHang.DaXacNhan => "info",
                TrangThaiDonHang.DangChuanBi => "primary",
                TrangThaiDonHang.DangGiao => "secondary",
                TrangThaiDonHang.DaGiao => "success",
                TrangThaiDonHang.DaHuy => "danger",
                _ => "dark"
            };
        }
    }
}