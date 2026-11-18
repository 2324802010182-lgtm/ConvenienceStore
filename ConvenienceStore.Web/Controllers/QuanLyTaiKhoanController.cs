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
        private readonly ILogger<QuanLyTaiKhoanController> _logger;

        public QuanLyTaiKhoanController(
            UserManager<NguoiDung> userManager,
            SignInManager<NguoiDung> signInManager,
            IDichVuDonHang dichVuDonHang,
            IDichVuEmail dichVuEmail,
            ILogger<QuanLyTaiKhoanController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dichVuDonHang = dichVuDonHang;
            _dichVuEmail = dichVuEmail;
            _logger = logger;
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
                    TongTien = x.TienGiamTuDiem > 0 ? x.TongTienSauGiam : x.TongTien,
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
                {
                    ModelState.AddModelError("", loi.Description);
                }

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
                    DiemDaSuDung = x.DiemDaSuDung,
                    TienGiamTuDiem = x.TienGiamTuDiem,
                    TongTienSauGiam = x.TongTienSauGiam,

                    NgayDatHang = x.NgayDatHang,
                    TrangThai = LayTenTrangThai(x.TrangThai),
                    PhuongThucThanhToan = LayTenPhuongThucThanhToan(x.PhuongThucThanhToan),
                    TrangThaiThanhToan = LayTenTrangThaiThanhToan(x.TrangThaiThanhToan)
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
                DiemDaSuDung = donHang.DiemDaSuDung,
                TienGiamTuDiem = donHang.TienGiamTuDiem,
                TongTienSauGiam = donHang.TongTienSauGiam,

                NgayDatHang = donHang.NgayDatHang,
                TrangThai = LayTenTrangThai(donHang.TrangThai),
                PhuongThucThanhToan = LayTenPhuongThucThanhToan(donHang.PhuongThucThanhToan),
                TrangThaiThanhToan = LayTenTrangThaiThanhToan(donHang.TrangThaiThanhToan),

                DanhSachSanPham = donHang.ChiTietDonHangs?.Select(ct => new ChiTietSanPhamTrongDonViewModel
                {
                    TenSanPham = !string.IsNullOrWhiteSpace(ct.TenHienThi) ? ct.TenHienThi : (ct.SanPham != null ? ct.SanPham.TenSanPham : ""),
                    HinhAnh = !string.IsNullOrWhiteSpace(ct.HinhAnhHienThi) ? ct.HinhAnhHienThi : (ct.SanPham != null ? ct.SanPham.HinhAnh : null),
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

            // Generate a short numeric OTP and a password-reset token, store both as user tokens
            var otp = new Random().Next(100000, 999999).ToString();

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(nguoiDung);
            var resetTokenMaHoa = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));

            var expiry = DateTime.UtcNow.AddMinutes(10);

            await _userManager.SetAuthenticationTokenAsync(nguoiDung, "ConvenienceStore", "PasswordResetOtp", otp);
            await _userManager.SetAuthenticationTokenAsync(nguoiDung, "ConvenienceStore", "PasswordResetToken", resetTokenMaHoa);
            await _userManager.SetAuthenticationTokenAsync(nguoiDung, "ConvenienceStore", "PasswordResetOtpExpiry", expiry.Ticks.ToString());

            var noiDung = $@"
                <h3>Đổi mật khẩu tài khoản</h3>
                <p>Bạn đã yêu cầu đổi mật khẩu cho tài khoản Convenience Store.</p>
                <p>Mã xác thực (OTP) của bạn: <strong>{HtmlEncoder.Default.Encode(otp)}</strong></p>
                <p>Mã có hiệu lực trong 10 phút.</p>";

            try
            {
                await _dichVuEmail.GuiEmailAsync(
                    nguoiDung.Email!,
                    "Mã OTP đổi mật khẩu",
                    noiDung);
            }
            catch (Exception ex)
            {
                // Ghi chi tiet loi vao log server, khong hien thi thong tin SMTP nhay cam cho nguoi dung.
                _logger.LogError(ex, "Khong the gui email OTP doi mat khau cho tai khoan {UserId}.", nguoiDung.Id);

                // Huy OTP va reset token chua gui duoc, tranh su dung lai ma khong den hop thu.
                await _userManager.RemoveAuthenticationTokenAsync(nguoiDung, "ConvenienceStore", "PasswordResetOtp");
                await _userManager.RemoveAuthenticationTokenAsync(nguoiDung, "ConvenienceStore", "PasswordResetToken");
                await _userManager.RemoveAuthenticationTokenAsync(nguoiDung, "ConvenienceStore", "PasswordResetOtpExpiry");

                ModelState.AddModelError("", "Không thể gửi email OTP. Vui lòng kiểm tra cấu hình email và thử lại sau.");
                var viewModel = new ViewModels.DoiMatKhauTaiKhoanViewModel
                {
                    Email = nguoiDung.Email ?? string.Empty
                };
                return View("DoiMatKhau", viewModel);
            }

            return RedirectToAction(nameof(NhapOtpDoiMatKhau));
        }

        [HttpGet]
        public async Task<IActionResult> NhapOtpDoiMatKhau()
        {
            var nguoiDung = await LayNguoiDungHienTaiAsync();

            if (nguoiDung == null)
                return Challenge();

            var model = new ViewModels.DoiMatKhauOtpViewModel
            {
                Email = nguoiDung.Email ?? string.Empty
            };

            ViewBag.MenuHienTai = "DoiMatKhau";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanOtpDoiMatKhau(ViewModels.DoiMatKhauOtpViewModel model)
        {
            ViewBag.MenuHienTai = "DoiMatKhau";

            if (!ModelState.IsValid)
                return View("NhapOtpDoiMatKhau", model);

            var nguoiDung = await LayNguoiDungHienTaiAsync();

            if (nguoiDung == null)
                return Challenge();

            var storedOtp = await _userManager.GetAuthenticationTokenAsync(nguoiDung, "ConvenienceStore", "PasswordResetOtp");
            var storedToken = await _userManager.GetAuthenticationTokenAsync(nguoiDung, "ConvenienceStore", "PasswordResetToken");
            var expiryTicks = await _userManager.GetAuthenticationTokenAsync(nguoiDung, "ConvenienceStore", "PasswordResetOtpExpiry");

            if (string.IsNullOrEmpty(storedOtp) || string.IsNullOrEmpty(storedToken) || string.IsNullOrEmpty(expiryTicks))
            {
                ModelState.AddModelError("", "Không có yêu cầu đổi mật khẩu hợp lệ. Vui lòng thử gửi lại mã.");
                return View("NhapOtpDoiMatKhau", model);
            }

            if (!long.TryParse(expiryTicks, out var ticks))
            {
                ModelState.AddModelError("", "Dữ liệu không hợp lệ. Vui lòng thử lại.");
                return View("NhapOtpDoiMatKhau", model);
            }

            var expiry = new DateTime(ticks, DateTimeKind.Utc);

            if (DateTime.UtcNow > expiry)
            {
                ModelState.AddModelError("", "Mã OTP đã hết hạn. Vui lòng gửi lại mã.");
                return View("NhapOtpDoiMatKhau", model);
            }

            if (model.Otp != storedOtp)
            {
                ModelState.AddModelError("", "Mã OTP không đúng.");
                return View("NhapOtpDoiMatKhau", model);
            }

            // Decode stored reset token and perform password reset
            string resetTokenGoc;

            try
            {
                resetTokenGoc = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(storedToken));
            }
            catch
            {
                ModelState.AddModelError("", "Liên kết đặt lại mật khẩu không hợp lệ.");
                return View("NhapOtpDoiMatKhau", model);
            }

            var ketQua = await _userManager.ResetPasswordAsync(nguoiDung, resetTokenGoc, model.MatKhauMoi!);

            if (ketQua.Succeeded)
            {
                // Mat khau thay doi: xoa danh sach trinh duyet tin cay de nhan dien lai o lan dang nhap sau.
                await _userManager.RemoveAuthenticationTokenAsync(
                    nguoiDung,
                    "ConvenienceStore.Security",
                    "TrustedDevicesV1");
                Response.Cookies.Delete("ConvenienceStore.TrustedDevice");

                await _userManager.RemoveAuthenticationTokenAsync(nguoiDung, "ConvenienceStore", "PasswordResetOtp");
                await _userManager.RemoveAuthenticationTokenAsync(nguoiDung, "ConvenienceStore", "PasswordResetToken");
                await _userManager.RemoveAuthenticationTokenAsync(nguoiDung, "ConvenienceStore", "PasswordResetOtpExpiry");

                TempData["ThanhCong"] = "Đổi mật khẩu thành công.";
                return RedirectToAction(nameof(DoiMatKhau));
            }

            foreach (var loi in ketQua.Errors)
            {
                ModelState.AddModelError("", loi.Description);
            }

            return View("NhapOtpDoiMatKhau", model);
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

        private string LayTenPhuongThucThanhToan(PhuongThucThanhToan phuongThuc)
        {
            return phuongThuc switch
            {
                PhuongThucThanhToan.ThanhToanKhiNhanHang => "Thanh toán khi nhận hàng",
                PhuongThucThanhToan.ChuyenKhoanNganHang => "Chuyển khoản ngân hàng",
                _ => "Không xác định"
            };
        }

        private string LayTenTrangThaiThanhToan(TrangThaiThanhToan trangThai)
        {
            return trangThai switch
            {
                TrangThaiThanhToan.ChuaThanhToan => "Chưa thanh toán",
                TrangThaiThanhToan.DaThanhToan => "Đã thanh toán",
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
