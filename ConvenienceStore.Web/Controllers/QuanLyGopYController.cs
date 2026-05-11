using ConvenienceStore.DataAccess;
using ConvenienceStore.Models.Entities;
using ConvenienceStore.Models.Enums;
using ConvenienceStore.Models.HangSo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ConvenienceStore.Web.Controllers
{
    [Authorize(Roles = VaiTro.Admin + "," + VaiTro.NhanVien)]
    public class QuanLyGopYController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuanLyGopYController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var isAdmin = User.IsInRole(VaiTro.Admin);

            var query = _context.GopYs
                .Include(x => x.NguoiGui)
                .Include(x => x.SanPham)
                .Include(x => x.NhanVien)
                .Include(x => x.TinNhans)
                .AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(x => x.LoaiGopY == LoaiGopY.SanPham);
            }

            var danhSach = await query
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();

            return View(danhSach);
        }

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

            var isNhanVien = User.IsInRole(VaiTro.NhanVien);

            if (isNhanVien && gopY.LoaiGopY == LoaiGopY.NhanVien)
            {
                return Forbid();
            }

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

            var isNhanVien = User.IsInRole(VaiTro.NhanVien);
            if (isNhanVien && gopY.LoaiGopY == LoaiGopY.NhanVien)
            {
                return Forbid();
            }

            var nguoiGuiId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(nguoiGuiId))
                return Challenge();

            var tinNhan = new TinNhanGopY
            {
                GopYId = gopYId,
                NguoiGuiId = nguoiGuiId,
                NoiDung = noiDung.Trim(),
                NgayGui = DateTime.Now
            };

            _context.TinNhanGopYs.Add(tinNhan);
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = "Đã gửi phản hồi cho khách hàng.";
            return RedirectToAction(nameof(ChiTiet), new { id = gopYId });
        }
    }
}