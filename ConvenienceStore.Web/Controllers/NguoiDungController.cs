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
    public class NguoiDungController : Controller
    {
        private readonly UserManager<NguoiDung> _userManager;

        public NguoiDungController(UserManager<NguoiDung> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var danhSachNguoiDung = await _userManager.Users
                .OrderBy(x => x.Email)
                .ToListAsync();

            var ketQua = new List<NguoiDungAdminViewModel>();

            foreach (var nguoiDung in danhSachNguoiDung)
            {
                var vaiTros = await _userManager.GetRolesAsync(nguoiDung);
                var vaiTro = vaiTros.FirstOrDefault() ?? VaiTro.KhachHang;

                bool biKhoa = nguoiDung.LockoutEnd.HasValue && nguoiDung.LockoutEnd > DateTimeOffset.Now;

                ketQua.Add(new NguoiDungAdminViewModel
                {
                    Id = nguoiDung.Id,
                    HoTen = nguoiDung.HoTen,
                    Email = nguoiDung.Email,
                    VaiTro = vaiTro,
                    BiKhoa = biKhoa,
                    KhoaDenNgay = nguoiDung.LockoutEnd
                });
            }

            return View(ketQua);
        }

        [HttpPost]
        public async Task<IActionResult> Khoa(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction(nameof(Index));

            var nguoiDung = await _userManager.FindByIdAsync(id);
            if (nguoiDung == null)
                return RedirectToAction(nameof(Index));

            if (nguoiDung.Email == "admin@akdstore.com")
            {
                TempData["Loi"] = "Không thể khóa tài khoản admin mặc định.";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.SetLockoutEnabledAsync(nguoiDung, true);
            await _userManager.SetLockoutEndDateAsync(nguoiDung, DateTimeOffset.Now.AddYears(100));

            TempData["ThanhCong"] = "Đã khóa tài khoản thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MoKhoa(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction(nameof(Index));

            var nguoiDung = await _userManager.FindByIdAsync(id);
            if (nguoiDung == null)
                return RedirectToAction(nameof(Index));

            await _userManager.SetLockoutEndDateAsync(nguoiDung, null);

            TempData["ThanhCong"] = "Đã mở khóa tài khoản thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}