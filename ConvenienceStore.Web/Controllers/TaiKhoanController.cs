using ConvenienceStore.Models.Entities;
using ConvenienceStore.Models.HangSo;
using ConvenienceStore.Web.Services;
using ConvenienceStore.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ConvenienceStore.Web.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly UserManager<NguoiDung> _userManager;
        private readonly SignInManager<NguoiDung> _signInManager;
        private readonly IDichVuEmail _dichVuEmail;
        private readonly ILogger<TaiKhoanController> _logger;
        private readonly IRecaptchaVerificationService _recaptchaService;
        private readonly RecaptchaSettings _recaptchaSettings;

        private const string TrustedDeviceCookieName = "ConvenienceStore.TrustedDevice";
        private const string TrustedDeviceLoginProvider = "ConvenienceStore.Security";
        private const string TrustedDeviceTokenName = "TrustedDevicesV1";
        private static readonly TimeSpan TrustedDeviceLifetime = TimeSpan.FromDays(180);

        public TaiKhoanController(
            UserManager<NguoiDung> userManager,
            SignInManager<NguoiDung> signInManager,
            IDichVuEmail dichVuEmail,
            ILogger<TaiKhoanController> logger,
            IRecaptchaVerificationService recaptchaService,
            IOptions<RecaptchaSettings> recaptchaSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dichVuEmail = dichVuEmail;
            _logger = logger;
            _recaptchaService = recaptchaService;
            _recaptchaSettings = recaptchaSettings.Value;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(DangKyViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var kiemTraEmail = await _userManager.FindByEmailAsync(model.Email);
            if (kiemTraEmail != null)
            {
                ModelState.AddModelError("", "Email này đã tồn tại.");
                return View(model);
            }

            var nguoiDung = new NguoiDung
            {
                UserName = model.Email,
                Email = model.Email,
                HoTen = model.HoTen,
                DiaChi = model.DiaChi,
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            var ketQua = await _userManager.CreateAsync(nguoiDung, model.MatKhau);

            if (ketQua.Succeeded)
            {
                await _userManager.AddToRoleAsync(nguoiDung, VaiTro.KhachHang);
                await _signInManager.SignInAsync(nguoiDung, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var loi in ketQua.Errors)
            {
                ModelState.AddModelError("", loi.Description);
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult DangNhap(string? returnUrl = null)
        {
            GanDuLieuTrangDangNhap(returnUrl);
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(
            DangNhapViewModel model,
            string? returnUrl = null,
            [FromForm(Name = "g-recaptcha-response")] string? recaptchaResponse = null)
        {
            if (!ModelState.IsValid)
            {
                GanDuLieuTrangDangNhap(returnUrl);
                return View(model);
            }

            if (!_recaptchaService.IsConfigured)
            {
                ModelState.AddModelError("", "reCAPTCHA chưa được cấu hình. Vui lòng liên hệ quản trị viên.");
                GanDuLieuTrangDangNhap(returnUrl);
                return View(model);
            }

            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            var captchaHopLe = await _recaptchaService.VerifyAsync(
                recaptchaResponse,
                remoteIp,
                Request.Headers.UserAgent.ToString(),
                Request.Host.Host,
                HttpContext.RequestAborted);

            if (!captchaHopLe)
            {
                ModelState.AddModelError("", "Vui lòng xác nhận reCAPTCHA hợp lệ trước khi đăng nhập.");
                GanDuLieuTrangDangNhap(returnUrl);
                return View(model);
            }

            var nguoiDung = await _userManager.FindByEmailAsync(model.Email);
            if (nguoiDung == null)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                GanDuLieuTrangDangNhap(returnUrl);
                return View(model);
            }

            // Dam bao ca tai khoan cu cung duoc phep ap dung co che khoa cua Identity.
            if (!await _userManager.GetLockoutEnabledAsync(nguoiDung))
            {
                var enableLockoutResult = await _userManager.SetLockoutEnabledAsync(nguoiDung, true);
                if (!enableLockoutResult.Succeeded)
                {
                    _logger.LogError("Could not enable lockout for UserId {UserId}.", nguoiDung.Id);
                    ModelState.AddModelError("", "Không thể xác thực đăng nhập an toàn lúc này. Vui lòng thử lại sau.");
                    GanDuLieuTrangDangNhap(returnUrl);
                    return View(model);
                }
            }

            var lockoutEndHienTai = await _userManager.GetLockoutEndDateAsync(nguoiDung);
            if (lockoutEndHienTai.HasValue && lockoutEndHienTai.Value > DateTimeOffset.UtcNow)
            {
                var conLai = lockoutEndHienTai.Value - DateTimeOffset.UtcNow;
                var giayConLai = Math.Max(1, (int)Math.Ceiling(conLai.TotalSeconds));
                ModelState.AddModelError("", $"Tài khoản đang tạm khóa. Vui lòng thử lại sau {giayConLai} giây.");
                GanDuLieuTrangDangNhap(returnUrl);
                return View(model);
            }

            var ketQua = await _signInManager.PasswordSignInAsync(
                nguoiDung.UserName!,
                model.MatKhau,
                model.GhiNhoDangNhap,
                lockoutOnFailure: true);

            if (ketQua.Succeeded)
            {
                // Dang nhap thanh cong se xoa cap khoa tang dan va Identity tu reset dem sai.
                await _userManager.RemoveAuthenticationTokenAsync(
                    nguoiDung,
                    ProgressiveLockoutLoginProvider,
                    ProgressiveLockoutStageTokenName);

                // Chỉ gửi cảnh báo sau khi thông tin đăng nhập đã được xác thực thành công.
                await KiemTraThietBiMoiVaGuiCanhBaoAsync(nguoiDung);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                if (await _userManager.IsInRoleAsync(nguoiDung, VaiTro.Admin) ||
                    await _userManager.IsInRoleAsync(nguoiDung, VaiTro.NhanVien))
                {
                    return RedirectToAction("Index", "QuanTri");
                }

                return RedirectToAction("Index", "Home");
            }

            if (ketQua.IsLockedOut)
            {
                var lockoutDuration = await TinhThoiGianKhoaTiepTheoAsync(nguoiDung);
                var lockoutEnd = DateTimeOffset.UtcNow.Add(lockoutDuration);
                var capNhatKhoa = await _userManager.SetLockoutEndDateAsync(nguoiDung, lockoutEnd);

                if (!capNhatKhoa.Succeeded)
                    _logger.LogError("Could not set progressive lockout end for UserId {UserId}.", nguoiDung.Id);

                ModelState.AddModelError("", $"Bạn đã nhập sai mật khẩu 5 lần. Tài khoản bị khóa {Math.Ceiling(lockoutDuration.TotalSeconds)} giây.");
                GanDuLieuTrangDangNhap(returnUrl);
                return View(model);
            }

            var soLanSai = await _userManager.GetAccessFailedCountAsync(nguoiDung);
            var soLanConLai = Math.Max(0, 5 - soLanSai);
            ModelState.AddModelError("", $"Email hoặc mật khẩu không đúng. Bạn còn {soLanConLai} lần thử trước khi tài khoản bị khóa 30 giây.");
            GanDuLieuTrangDangNhap(returnUrl);
            return View(model);
        }

        private const string ProgressiveLockoutLoginProvider = "ConvenienceStore.Security";
        private const string ProgressiveLockoutStageTokenName = "ProgressiveLoginLockoutStageV1";
        private const long MaximumLockoutSeconds = 24 * 60 * 60;

        private async Task<TimeSpan> TinhThoiGianKhoaTiepTheoAsync(NguoiDung nguoiDung)
        {
            var stageValue = await _userManager.GetAuthenticationTokenAsync(
                nguoiDung,
                ProgressiveLockoutLoginProvider,
                ProgressiveLockoutStageTokenName);

            var stage = int.TryParse(stageValue, out var parsedStage)
                ? Math.Clamp(parsedStage, 0, 20)
                : 0;

            long seconds = 30;
            for (var i = 0; i < stage && seconds < MaximumLockoutSeconds; i++)
                seconds = Math.Min(seconds * 2, MaximumLockoutSeconds);

            var nextStage = Math.Min(stage + 1, 20);
            var saved = await _userManager.SetAuthenticationTokenAsync(
                nguoiDung,
                ProgressiveLockoutLoginProvider,
                ProgressiveLockoutStageTokenName,
                nextStage.ToString());

            if (!saved.Succeeded)
                _logger.LogError("Could not save progressive lockout stage for UserId {UserId}.", nguoiDung.Id);

            return TimeSpan.FromSeconds(seconds);
        }

        private void GanDuLieuTrangDangNhap(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.RecaptchaSiteKey = _recaptchaSettings.SiteKey;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DangXuat()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult QuenMatKhau()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuenMatKhau(QuenMatKhauViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var nguoiDung = await _userManager.FindByEmailAsync(model.Email);

            if (nguoiDung == null)
            {
                return RedirectToAction(nameof(XacNhanGuiEmail));
            }

            try
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(nguoiDung);
                var tokenMaHoa = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                var linkDatLaiMatKhau = Url.Action(
                    nameof(DatLaiMatKhau),
                    "TaiKhoan",
                    new { token = tokenMaHoa, email = nguoiDung.Email },
                    Request.Scheme);

                if (string.IsNullOrWhiteSpace(linkDatLaiMatKhau))
                    throw new InvalidOperationException("Không thể tạo liên kết đặt lại mật khẩu.");

                var linkDaMaHoa = HtmlEncoder.Default.Encode(linkDatLaiMatKhau);
                var noiDung = $@"
                    <h3>Đặt lại mật khẩu Convenience Store</h3>
                    <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                    <p>Liên kết đặt lại mật khẩu chỉ dùng một lần và có thời hạn theo cấu hình Identity.</p>
                    <p><a href='{linkDaMaHoa}'>Đặt lại mật khẩu</a></p>
                    <p>Nếu bạn không yêu cầu thao tác này, hãy bỏ qua email. Mật khẩu hiện tại sẽ không thay đổi.</p>";

                await _dichVuEmail.GuiEmailAsync(
                    nguoiDung.Email!,
                    "Đặt lại mật khẩu Convenience Store",
                    noiDung);
            }
            catch (Exception ex)
            {
                // Không tiết lộ email có tồn tại hay không và không làm lộ lỗi SMTP cho người dùng.
                _logger.LogError(ex, "Không thể gửi email đặt lại mật khẩu cho UserId {UserId}.", nguoiDung.Id);
            }

            return RedirectToAction(nameof(XacNhanGuiEmail));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult XacNhanGuiEmail()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult DatLaiMatKhau(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
                return RedirectToAction("Index", "Home");

            var model = new DatLaiMatKhauViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DatLaiMatKhau(DatLaiMatKhauViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var nguoiDung = await _userManager.FindByEmailAsync(model.Email);

            if (nguoiDung == null)
            {
                ModelState.AddModelError("", "Không tìm thấy người dùng");
                return View(model);
            }

            string tokenGoc;

            try
            {
                tokenGoc = Encoding.UTF8.GetString(
                    WebEncoders.Base64UrlDecode(model.Token));
            }
            catch
            {
                ModelState.AddModelError("", "Liên kết đặt lại mật khẩu không hợp lệ.");
                return View(model);
            }

            var ketQua = await _userManager.ResetPasswordAsync(
                nguoiDung,
                tokenGoc,
                model.MatKhauMoi);

            if (ketQua.Succeeded)
            {
                // Sau khi đặt lại mật khẩu, yêu cầu mọi trình duyệt phải được nhận diện lại.
                await _userManager.RemoveAuthenticationTokenAsync(
                    nguoiDung,
                    TrustedDeviceLoginProvider,
                    TrustedDeviceTokenName);
                Response.Cookies.Delete(TrustedDeviceCookieName);

                return RedirectToAction(nameof(XacNhanDatLaiMatKhau));
            }

            foreach (var loi in ketQua.Errors)
            {
                ModelState.AddModelError("", loi.Description);
            }

            return View(model);
        }

        /// <summary>
        /// Nhận diện trình duyệt đã được ghi nhận bằng cookie ngẫu nhiên.
        /// Chỉ lưu SHA-256 của token trong AspNetUserTokens, không lưu token gốc.
        /// Đây là nhận diện trình duyệt, không phải định danh phần cứng tuyệt đối.
        /// </summary>
        private async Task KiemTraThietBiMoiVaGuiCanhBaoAsync(NguoiDung nguoiDung)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nguoiDung.Email))
                    return;

                var jsonThietBi = await _userManager.GetAuthenticationTokenAsync(
                    nguoiDung,
                    TrustedDeviceLoginProvider,
                    TrustedDeviceTokenName);

                List<TrustedDeviceEntry> danhSachThietBi;
                try
                {
                    danhSachThietBi = string.IsNullOrWhiteSpace(jsonThietBi)
                        ? new List<TrustedDeviceEntry>()
                        : JsonSerializer.Deserialize<List<TrustedDeviceEntry>>(jsonThietBi) ?? new List<TrustedDeviceEntry>();
                }
                catch (JsonException)
                {
                    _logger.LogWarning("Danh sách thiết bị tin cậy không hợp lệ của UserId {UserId}; tạo danh sách mới.", nguoiDung.Id);
                    danhSachThietBi = new List<TrustedDeviceEntry>();
                }

                var now = DateTimeOffset.UtcNow;
                danhSachThietBi.RemoveAll(x => x.ExpiresAtUtc <= now);

                var tokenCookieHienTai = Request.Cookies[TrustedDeviceCookieName];
                if (!string.IsNullOrWhiteSpace(tokenCookieHienTai))
                {
                    var hashCookie = TinhHashThietBi(tokenCookieHienTai);
                    var daTinCay = danhSachThietBi.Any(x =>
                        string.Equals(x.TokenHash, hashCookie, StringComparison.Ordinal));

                    if (daTinCay)
                        return;
                }

                var tokenThietBiMoi = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
                var hashThietBiMoi = TinhHashThietBi(tokenThietBiMoi);
                var userAgent = Request.Headers["User-Agent"].ToString();
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Không xác định";
                var moTaThietBi = MoTaTrinhDuyetVaHeDieuHanh(userAgent);
                var thoiGian = DateTimeOffset.Now;
                var linkQuenMatKhau = Url.Action(nameof(QuenMatKhau), "TaiKhoan", values: null, protocol: Request.Scheme);

                var noiDung = $@"
                    <h3>Cảnh báo đăng nhập từ thiết bị chưa được ghi nhận</h3>
                    <p>Tài khoản Convenience Store của bạn vừa đăng nhập thành công từ một trình duyệt chưa được ghi nhận trước đây.</p>
                    <ul>
                        <li><strong>Thời gian:</strong> {HtmlEncoder.Default.Encode(thoiGian.ToString("dd/MM/yyyy HH:mm:ss zzz"))}</li>
                        <li><strong>Thiết bị/trình duyệt:</strong> {HtmlEncoder.Default.Encode(moTaThietBi)}</li>
                        <li><strong>Địa chỉ IP:</strong> {HtmlEncoder.Default.Encode(ip)}</li>
                        <li><strong>User-Agent:</strong> {HtmlEncoder.Default.Encode(string.IsNullOrWhiteSpace(userAgent) ? "Không xác định" : userAgent[..Math.Min(userAgent.Length, 300)])}</li>
                    </ul>
                    <p>Nếu chính bạn vừa đăng nhập, bạn không cần thực hiện thêm thao tác nào.</p>
                    <p><strong>Nếu không phải bạn:</strong> hãy đặt lại mật khẩu ngay tại <a href='{HtmlEncoder.Default.Encode(linkQuenMatKhau ?? string.Empty)}'>trang Quên mật khẩu</a> và liên hệ quản trị viên để kiểm tra tài khoản.</p>
                    <p>Không chia sẻ mật khẩu hoặc mã OTP cho bất kỳ ai.</p>";

                // Gửi cảnh báo trước khi đánh dấu trình duyệt là tin cậy.
                // Nếu SMTP lỗi, lần đăng nhập sau sẽ thử gửi cảnh báo lại.
                await _dichVuEmail.GuiEmailAsync(
                    nguoiDung.Email,
                    "Cảnh báo đăng nhập thiết bị mới - Convenience Store",
                    noiDung);

                danhSachThietBi.Add(new TrustedDeviceEntry
                {
                    TokenHash = hashThietBiMoi,
                    DeviceLabel = moTaThietBi,
                    AddedAtUtc = now,
                    ExpiresAtUtc = now.Add(TrustedDeviceLifetime)
                });

                // Giới hạn tối đa 10 trình duyệt được ghi nhận cho mỗi tài khoản.
                danhSachThietBi = danhSachThietBi
                    .OrderByDescending(x => x.AddedAtUtc)
                    .Take(10)
                    .ToList();

                var ketQuaLuu = await _userManager.SetAuthenticationTokenAsync(
                    nguoiDung,
                    TrustedDeviceLoginProvider,
                    TrustedDeviceTokenName,
                    JsonSerializer.Serialize(danhSachThietBi));

                if (!ketQuaLuu.Succeeded)
                {
                    _logger.LogWarning("Không thể lưu thiết bị tin cậy cho UserId {UserId}.", nguoiDung.Id);
                    return;
                }

                Response.Cookies.Append(
                    TrustedDeviceCookieName,
                    tokenThietBiMoi,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = Request.IsHttps,
                        SameSite = SameSiteMode.Lax,
                        IsEssential = true,
                        Expires = now.Add(TrustedDeviceLifetime),
                        Path = "/"
                    });
            }
            catch (Exception ex)
            {
                // Không làm hỏng luồng đăng nhập nếu dịch vụ email/nhận diện thiết bị gặp lỗi.
                _logger.LogError(ex, "Không thể kiểm tra hoặc gửi cảnh báo thiết bị mới cho UserId {UserId}.", nguoiDung.Id);
            }
        }

        private static string TinhHashThietBi(string token)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hash);
        }

        private static string MoTaTrinhDuyetVaHeDieuHanh(string userAgent)
        {
            var browser = userAgent.Contains("Edg/", StringComparison.OrdinalIgnoreCase) ? "Microsoft Edge"
                : userAgent.Contains("Firefox/", StringComparison.OrdinalIgnoreCase) ? "Mozilla Firefox"
                : userAgent.Contains("Chrome/", StringComparison.OrdinalIgnoreCase) ? "Google Chrome"
                : userAgent.Contains("Safari/", StringComparison.OrdinalIgnoreCase) ? "Safari"
                : "Trình duyệt không xác định";

            var operatingSystem = userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase) ? "Windows"
                : userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase) ? "Android"
                : userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase) || userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase) ? "iOS/iPadOS"
                : userAgent.Contains("Mac OS", StringComparison.OrdinalIgnoreCase) ? "macOS"
                : userAgent.Contains("Linux", StringComparison.OrdinalIgnoreCase) ? "Linux"
                : "hệ điều hành không xác định";

            return $"{browser} trên {operatingSystem}";
        }

        private sealed class TrustedDeviceEntry
        {
            public string TokenHash { get; set; } = string.Empty;
            public string DeviceLabel { get; set; } = string.Empty;
            public DateTimeOffset AddedAtUtc { get; set; }
            public DateTimeOffset ExpiresAtUtc { get; set; }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult XacNhanDatLaiMatKhau()
        {
            return View();
        }

        [HttpGet]
        public IActionResult TuChoiTruyCap()
        {
            return View();
        }
    }
}
