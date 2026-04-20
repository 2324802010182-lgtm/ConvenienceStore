using ConvenienceStore.Models.Entities;
using ConvenienceStore.Models.HangSo;
using ConvenienceStore.Web.Services;
using ConvenienceStore.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;

namespace ConvenienceStore.Web.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly UserManager<NguoiDung> _userManager;
        private readonly SignInManager<NguoiDung> _signInManager;
        private readonly IDichVuEmail _dichVuEmail;

        public TaiKhoanController(
            UserManager<NguoiDung> userManager,
            SignInManager<NguoiDung> signInManager,
            IDichVuEmail dichVuEmail)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dichVuEmail = dichVuEmail;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
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
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> DangNhap(DangNhapViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var nguoiDung = await _userManager.FindByEmailAsync(model.Email);
            if (nguoiDung == null)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var ketQua = await _signInManager.PasswordSignInAsync(
                nguoiDung.UserName!,
                model.MatKhau,
                model.GhiNhoDangNhap,
                lockoutOnFailure: false);

            if (ketQua.Succeeded)
            {
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
                ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa.");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
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
        public async Task<IActionResult> QuenMatKhau(QuenMatKhauViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var nguoiDung = await _userManager.FindByEmailAsync(model.Email);

            if (nguoiDung == null)
            {
                return RedirectToAction(nameof(XacNhanGuiEmail));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(nguoiDung);
            var tokenMaHoa = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var linkDatLaiMatKhau = Url.Action(
                nameof(DatLaiMatKhau),
                "TaiKhoan",
                new { token = tokenMaHoa, email = nguoiDung.Email },
                Request.Scheme);

            var noiDung = $@"
                <h3>Đặt lại mật khẩu</h3>
                <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản Convenience Store.</p>
                <p>Vui lòng bấm vào liên kết bên dưới để đặt lại mật khẩu:</p>
                <p>
                    <a href='{HtmlEncoder.Default.Encode(linkDatLaiMatKhau!)}'>
                        Đặt lại mật khẩu
                    </a>
                </p>
                <p>Nếu không bấm được, hãy copy link này vào trình duyệt:</p>
                <p>{HtmlEncoder.Default.Encode(linkDatLaiMatKhau!)}</p>";

            await _dichVuEmail.GuiEmailAsync(
                nguoiDung.Email!,
                "Đặt lại mật khẩu",
                noiDung);

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
                return RedirectToAction(nameof(XacNhanDatLaiMatKhau));
            }

            foreach (var loi in ketQua.Errors)
            {
                ModelState.AddModelError("", loi.Description);
            }

            return View(model);
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