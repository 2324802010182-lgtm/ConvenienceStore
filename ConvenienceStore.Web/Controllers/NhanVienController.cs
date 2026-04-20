using ConvenienceStore.Models.Entities;
using ConvenienceStore.Models.HangSo;
using ConvenienceStore.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ConvenienceStore.Web.Controllers
{
    [Authorize(Roles = VaiTro.Admin)]
    public class NhanVienController : Controller
    {
        private readonly UserManager<NguoiDung> _userManager;

        public NhanVienController(UserManager<NguoiDung> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult TaoTaiKhoan()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TaoTaiKhoan(TaoTaiKhoanNhanVienViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userDaTonTai = await _userManager.FindByEmailAsync(model.Email);
            if (userDaTonTai != null)
            {
                ModelState.AddModelError("", "Email này đã tồn tại.");
                return View(model);
            }

            var nhanVien = new NguoiDung
            {
                UserName = model.Email,
                Email = model.Email,
                HoTen = model.HoTen,
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            var ketQua = await _userManager.CreateAsync(nhanVien, model.MatKhau);

            if (!ketQua.Succeeded)
            {
                foreach (var loi in ketQua.Errors)
                {
                    ModelState.AddModelError("", loi.Description);
                }
                return View(model);
            }

            await _userManager.AddToRoleAsync(nhanVien, VaiTro.NhanVien);

            TempData["ThanhCong"] = "Tạo tài khoản nhân viên thành công.";
            return RedirectToAction(nameof(TaoTaiKhoan));
        }
    }
}