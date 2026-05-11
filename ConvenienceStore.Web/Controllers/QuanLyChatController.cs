using ConvenienceStore.DataAccess;
using ConvenienceStore.Models.Entities;
using ConvenienceStore.Models.Enums;
using ConvenienceStore.Models.HangSo;
using ConvenienceStore.Web.Hubs;
using ConvenienceStore.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ConvenienceStore.Web.Controllers
{
    [Authorize(Roles = VaiTro.Admin + "," + VaiTro.NhanVien)]
    public class QuanLyChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatOnlineHub> _hubContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public QuanLyChatController(
            ApplicationDbContext context,
            IHubContext<ChatOnlineHub> hubContext,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _hubContext = hubContext;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var danhSachHoiThoai = await _context.HoiThoaiChats
                .Include(x => x.KhachHang)
                .Include(x => x.NhanVienPhuTrach)
                .OrderByDescending(x => x.LanHoatDongCuoi)
                .ToListAsync();

            return View(danhSachHoiThoai);
        }

        public async Task<IActionResult> ChiTiet(int id)
        {
            var hoiThoai = await _context.HoiThoaiChats
                .Include(x => x.KhachHang)
                .Include(x => x.NhanVienPhuTrach)
                .Include(x => x.TinNhans.OrderBy(t => t.NgayGui))
                    .ThenInclude(x => x.NguoiGui)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (hoiThoai == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (string.IsNullOrWhiteSpace(hoiThoai.NhanVienPhuTrachId))
            {
                hoiThoai.NhanVienPhuTrachId = userId;
            }

            hoiThoai.QuanTriDaDoc = true;
            hoiThoai.LanHoatDongCuoi = DateTime.Now;

            await _context.SaveChangesAsync();

            var model = new ChiTietChatViewModel
            {
                HoiThoai = hoiThoai,
                DanhSachTinNhan = hoiThoai.TinNhans.OrderBy(x => x.NgayGui).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiTinNhan(int hoiThoaiId, string? noiDung, IFormFile? tepHinhAnh)
        {
            bool khongCoNoiDung = string.IsNullOrWhiteSpace(noiDung);
            bool khongCoAnh = tepHinhAnh == null || tepHinhAnh.Length == 0;

            if (khongCoNoiDung && khongCoAnh)
            {
                TempData["LoiChat"] = "Vui lòng nhập nội dung hoặc chọn ảnh.";
                return RedirectToAction(nameof(ChiTiet), new { id = hoiThoaiId });
            }

            var hoiThoai = await _context.HoiThoaiChats
                .Include(x => x.KhachHang)
                .Include(x => x.NhanVienPhuTrach)
                .FirstOrDefaultAsync(x => x.Id == hoiThoaiId);

            if (hoiThoai == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var nguoiGui = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (string.IsNullOrWhiteSpace(hoiThoai.NhanVienPhuTrachId))
            {
                hoiThoai.NhanVienPhuTrachId = userId;
            }

            string? duongDanAnh = null;

            if (tepHinhAnh != null && tepHinhAnh.Length > 0)
            {
                duongDanAnh = await LuuAnhChatAsync(tepHinhAnh);
            }

            var tinNhan = new TinNhanChat
            {
                HoiThoaiChatId = hoiThoaiId,
                NguoiGuiId = userId,
                NoiDung = string.IsNullOrWhiteSpace(noiDung) ? null : noiDung.Trim(),
                HinhAnh = duongDanAnh,
                NgayGui = DateTime.Now
            };

            _context.TinNhanChats.Add(tinNhan);

            hoiThoai.LanHoatDongCuoi = DateTime.Now;
            hoiThoai.KhachHangDaDoc = false;
            hoiThoai.QuanTriDaDoc = true;
            hoiThoai.TrangThai = TrangThaiHoiThoaiChat.DangTraoDoi;

            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"CHAT_{hoiThoaiId}")
                .SendAsync("NhanTinNhanMoi", new
                {
                    hoiThoaiId = hoiThoaiId,
                    nguoiGui = nguoiGui?.HoTen ?? nguoiGui?.Email ?? "Hỗ trợ viên",
                    noiDung = tinNhan.NoiDung,
                    hinhAnh = tinNhan.HinhAnh,
                    thoiGian = tinNhan.NgayGui.ToString("dd/MM/yyyy HH:mm"),
                    laQuanTri = true
                });

            return RedirectToAction(nameof(ChiTiet), new { id = hoiThoaiId });
        }

        private async Task<string> LuuAnhChatAsync(IFormFile tepHinhAnh)
        {
            var thuMucLuu = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "chat-online");

            if (!Directory.Exists(thuMucLuu))
            {
                Directory.CreateDirectory(thuMucLuu);
            }

            var tenFileMoi = $"{Guid.NewGuid()}{Path.GetExtension(tepHinhAnh.FileName)}";
            var duongDanDayDu = Path.Combine(thuMucLuu, tenFileMoi);

            using (var stream = new FileStream(duongDanDayDu, FileMode.Create))
            {
                await tepHinhAnh.CopyToAsync(stream);
            }

            return $"/uploads/chat-online/{tenFileMoi}";
        }
    }
}