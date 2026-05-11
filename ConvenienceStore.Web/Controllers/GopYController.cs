using ConvenienceStore.DataAccess;
using ConvenienceStore.Models.Entities;
using ConvenienceStore.Models.Enums;
using ConvenienceStore.Models.HangSo;
using ConvenienceStore.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ConvenienceStore.Web.Controllers
{
    [Authorize]
    public class GopYController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<NguoiDung> _userManager;

        public GopYController(ApplicationDbContext context, UserManager<NguoiDung> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Tao()
        {
            var model = new TaoGopYViewModel();
            await NapDuLieuAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Tao(TaoGopYViewModel model)
        {
            var nguoiDungId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (model.LoaiGopY == (int)LoaiGopY.SanPham && model.SanPhamId == null)
                ModelState.AddModelError("", "Vui lòng chọn sản phẩm.");

            if (model.LoaiGopY == (int)LoaiGopY.NhanVien && string.IsNullOrEmpty(model.NhanVienId))
                ModelState.AddModelError("", "Vui lòng chọn nhân viên.");

            if (model.LoaiGopY == (int)LoaiGopY.SanPham)
            {
                var daMua = await _context.ChiTietDonHangs
                    .Include(x => x.DonHang)
                    .AnyAsync(x =>
                        x.SanPhamId == model.SanPhamId &&
                        x.DonHang != null &&
                        x.DonHang.NguoiDungId == nguoiDungId &&
                        x.DonHang.TrangThai == TrangThaiDonHang.DaGiao);

                if (!daMua)
                    ModelState.AddModelError("", "Bạn chỉ được góp ý về sản phẩm đã mua và đã giao.");
            }

            if (!ModelState.IsValid)
            {
                await NapDuLieuAsync(model);
                return View(model);
            }

            var gopY = new GopY
            {
                NguoiGuiId = nguoiDungId,
                LoaiGopY = (LoaiGopY)model.LoaiGopY,
                SanPhamId = model.SanPhamId,
                NhanVienId = model.NhanVienId,
                TieuDe = model.TieuDe,
                NoiDung = model.NoiDung,
                NgayTao = DateTime.Now,
                DaDong = false
            };

            _context.GopYs.Add(gopY);
            await _context.SaveChangesAsync();

            _context.TinNhanGopYs.Add(new TinNhanGopY
            {
                GopYId = gopY.Id,
                NguoiGuiId = nguoiDungId,
                NoiDung = model.NoiDung,
                NgayGui = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ChiTiet), new { id = gopY.Id });
        }

        [HttpGet]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var gopY = await _context.GopYs
                .Include(x => x.NguoiGui)
                .Include(x => x.SanPham)
                .Include(x => x.NhanVien)
                .Include(x => x.TinNhans)
                    .ThenInclude(x => x.NguoiGui)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (gopY == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isAdmin = User.IsInRole(VaiTro.Admin);
            var isNhanVien = User.IsInRole(VaiTro.NhanVien);

            if (!isAdmin && !isNhanVien && gopY.NguoiGuiId != userId)
                return Forbid();

            if (isNhanVien && gopY.LoaiGopY == LoaiGopY.NhanVien)
                return Forbid();

            return View(gopY);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiPhanHoi(int gopYId, string noiDung)
        {
            if (string.IsNullOrWhiteSpace(noiDung))
            {
                TempData["Loi"] = "Vui lòng nhập nội dung phản hồi.";
                return RedirectToAction(nameof(ChiTiet), new { id = gopYId });
            }

            var gopY = await _context.GopYs.FirstOrDefaultAsync(x => x.Id == gopYId);
            if (gopY == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (gopY.NguoiGuiId != userId)
                return Forbid();

            _context.TinNhanGopYs.Add(new TinNhanGopY
            {
                GopYId = gopYId,
                NguoiGuiId = userId,
                NoiDung = noiDung.Trim(),
                NgayGui = DateTime.Now
            });

            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = "Đã gửi phản hồi bổ sung.";
            return RedirectToAction(nameof(ChiTiet), new { id = gopYId });
        }

        [HttpGet]
        public async Task<IActionResult> CuaToi()
        {
            var nguoiDungId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var danhSach = await _context.GopYs
                .Include(x => x.SanPham)
                .Include(x => x.NhanVien)
                .Where(x => x.NguoiGuiId == nguoiDungId)
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();

            return View(danhSach);
        }

        private async Task NapDuLieuAsync(TaoGopYViewModel model)
        {
            var nguoiDungId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var sanPhamDaMua = await _context.ChiTietDonHangs
                .Include(x => x.DonHang)
                .Include(x => x.SanPham)
                .Where(x => x.DonHang != null &&
                            x.DonHang.NguoiDungId == nguoiDungId &&
                            x.DonHang.TrangThai == TrangThaiDonHang.DaGiao)
                .Select(x => x.SanPham!)
                .Distinct()
                .ToListAsync();

            model.DanhSachSanPham = sanPhamDaMua.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.TenSanPham
            }).ToList();

            var nhanVienIds = await _context.UserRoles
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { ur.UserId, r.Name })
                .Where(x => x.Name == VaiTro.NhanVien)
                .Select(x => x.UserId)
                .ToListAsync();

            var dsNhanVien = await _context.Users
                .Where(x => nhanVienIds.Contains(x.Id))
                .OrderBy(x => x.MaNhanVien)
                .ToListAsync();

            model.DanhSachNhanVien = dsNhanVien.Select(x => new SelectListItem
            {
                Value = x.Id,
                Text = $"{x.MaNhanVien} - {x.HoTen}"
            }).ToList();
        }
    }
}