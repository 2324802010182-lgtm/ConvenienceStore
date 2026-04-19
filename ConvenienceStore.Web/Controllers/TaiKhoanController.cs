using ConvenienceStore.Models.Entities;
using ConvenienceStore.Models.HangSo;
using ConvenienceStore.Web.Services;
using ConvenienceStore.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DangKy(DangKyViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var nguoiDung = new NguoiDung
            {
                UserName = model.Email,
                Email = model.Email,
                HoTen = model.HoTen,
                DiaChi = model.DiaChi
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
        public IActionResult DangNhap(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DangNhap(DangNhapViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var ketQua = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.MatKhau,
                model.GhiNhoDangNhap,
                lockoutOnFailure: false);

            if (ketQua.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            if (ketQua.IsLockedOut)
            {
                ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa.");
                return View(model);
            }

            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
            return View(model);

            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DangXuat()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult QuenMatKhau()
        {
            return View();
        }

        [HttpPost]
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

            var linkDatLaiMatKhau = Url.Action(
                nameof(DatLaiMatKhau),
                "TaiKhoan",
                new { token, email = nguoiDung.Email },
                Request.Scheme);

            var noiDung = $@"
                <h3>Đặt lại mật khẩu</h3>
                <p>Bạn hãy bấm vào liên kết bên dưới để đặt lại mật khẩu:</p>
                <p><a href='{HtmlEncoder.Default.Encode(linkDatLaiMatKhau)}'>Đặt lại mật khẩu</a></p>";

            await _dichVuEmail.GuiEmailAsync(nguoiDung.Email!, "Đặt lại mật khẩu", noiDung);

            return RedirectToAction(nameof(XacNhanGuiEmail));
        }

        [HttpGet]
        public IActionResult XacNhanGuiEmail()
        {
            return View();
        }

        [HttpGet]
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

            var ketQua = await _userManager.ResetPasswordAsync(nguoiDung, model.Token, model.MatKhauMoi);

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