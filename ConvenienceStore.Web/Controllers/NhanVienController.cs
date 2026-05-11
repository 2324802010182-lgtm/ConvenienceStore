using ConvenienceStore.DataAccess;
using ConvenienceStore.Models.Entities;
using ConvenienceStore.Models.HangSo;
using ConvenienceStore.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.Web.Controllers
{
    [Authorize(Roles = VaiTro.Admin)]
    public class NhanVienController : Controller
    {
        private readonly UserManager<NguoiDung> _userManager;
        private readonly ApplicationDbContext _context;

        public NhanVienController(
            UserManager<NguoiDung> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
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

            var kiemTraEmail = await _userManager.FindByEmailAsync(model.Email);
            if (kiemTraEmail != null)
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

            var soNhanVien = await _context.Users
                .CountAsync(x => x.MaNhanVien != null);

            nhanVien.MaNhanVien = $"NV{(soNhanVien + 1):D3}";
            await _userManager.UpdateAsync(nhanVien);

            TempData["ThanhCong"] = $"Tạo tài khoản nhân viên thành công. Mã nhân viên: {nhanVien.MaNhanVien}";
            return RedirectToAction(nameof(TaoTaiKhoan));
        }
    }
}