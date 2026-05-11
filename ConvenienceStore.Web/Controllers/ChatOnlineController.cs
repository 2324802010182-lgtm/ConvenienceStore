using ConvenienceStore.DataAccess;
using ConvenienceStore.Models.Entities;
using ConvenienceStore.Models.Enums;
using ConvenienceStore.Models.HangSo;
using ConvenienceStore.Web.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ConvenienceStore.Web.Controllers
{
    [Authorize]
    public class ChatOnlineController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatOnlineHub> _hubContext;

        public ChatOnlineController(ApplicationDbContext context, IHubContext<ChatOnlineHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var hoiThoai = await _context.HoiThoaiChats
                .Include(x => x.TinNhans)
                    .ThenInclude(x => x.NguoiGui)
                .Include(x => x.NhanVienPhuTrach)
                .FirstOrDefaultAsync(x => x.KhachHangId == userId && x.TrangThai != TrangThaiHoiThoaiChat.DaDong);

            if (hoiThoai == null)
            {
                hoiThoai = new HoiThoaiChat
                {
                    KhachHangId = userId,
                    TieuDe = "Hỗ trợ trực tuyến",
                    TrangThai = TrangThaiHoiThoaiChat.ChoHoTro,
                    NgayTao = DateTime.Now,
                    LanHoatDongCuoi = DateTime.Now,
                    KhachHangDaDoc = true,
                    QuanTriDaDoc = false
                };

                _context.HoiThoaiChats.Add(hoiThoai);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group("NHOM_HO_TRO")
                    .SendAsync("CoHoiThoaiMoi", new
                    {
                        hoiThoaiId = hoiThoai.Id,
                        thongBao = "Có khách hàng mới cần hỗ trợ."
                    });
            }

            return View(hoiThoai);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiTinNhan(int hoiThoaiId, string noiDung)
        {
            if (string.IsNullOrWhiteSpace(noiDung))
                return RedirectToAction(nameof(Index));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var hoiThoai = await _context.HoiThoaiChats
                .FirstOrDefaultAsync(x => x.Id == hoiThoaiId && x.KhachHangId == userId);

            if (hoiThoai == null)
                return NotFound();

            var nguoiGui = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            var tinNhan = new TinNhanChat
            {
                HoiThoaiChatId = hoiThoaiId,
                NguoiGuiId = userId,
                NoiDung = noiDung.Trim(),
                NgayGui = DateTime.Now
            };

            _context.TinNhanChats.Add(tinNhan);

            hoiThoai.LanHoatDongCuoi = DateTime.Now;
            hoiThoai.QuanTriDaDoc = false;
            hoiThoai.KhachHangDaDoc = true;

            if (hoiThoai.TrangThai == TrangThaiHoiThoaiChat.ChoHoTro)
            {
                hoiThoai.TrangThai = TrangThaiHoiThoaiChat.DangTraoDoi;
            }

            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"CHAT_{hoiThoaiId}")
                .SendAsync("NhanTinNhanMoi", new
                {
                    hoiThoaiId = hoiThoaiId,
                    nguoiGui = nguoiGui?.HoTen ?? nguoiGui?.Email ?? "Khách hàng",
                    noiDung = tinNhan.NoiDung,
                    thoiGian = tinNhan.NgayGui.ToString("dd/MM/yyyy HH:mm"),
                    laQuanTri = false
                });

            await _hubContext.Clients.Group("NHOM_HO_TRO")
                .SendAsync("CoTinNhanMoiChoHoTro", new
                {
                    hoiThoaiId = hoiThoaiId,
                    thongBao = "Khách hàng vừa gửi tin nhắn mới."
                });

            return RedirectToAction(nameof(Index));
        }
    }
}