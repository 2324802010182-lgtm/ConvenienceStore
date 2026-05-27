using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.DataAccess;
using ConvenienceStore.Models.Entities;
using ConvenienceStore.Models.HangSo;
using ConvenienceStore.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.Web.Controllers
{
    [Authorize(Roles = VaiTro.Admin + "," + VaiTro.NhanVien)]
    public class KhuyenMaiController : Controller
    {
        private readonly IDichVuSanPham _dichVuSanPham;
        private readonly ApplicationDbContext _context;

        public KhuyenMaiController(IDichVuSanPham dichVuSanPham, ApplicationDbContext context)
        {
            _dichVuSanPham = dichVuSanPham;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var danhSachSanPham = await _dichVuSanPham.LayTatCaKemDanhMucAsync();
            var danhSachCombo = await _context.Combos
                .Include(x => x.ComboSanPhams)
                    .ThenInclude(x => x.SanPham)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            var model = new QuanLyKhuyenMaiViewModel
            {
                DanhSachCombo = danhSachCombo.Select(combo => new ComboKhuyenMaiViewModel
                {
                    Id = combo.Id,
                    TenCombo = combo.TenCombo,
                    DanhSachSanPham = string.Join(", ", combo.ComboSanPhams.Select(x => x.SanPham?.TenSanPham).Where(x => !string.IsNullOrWhiteSpace(x))),
                    GiaGoc = combo.GiaGoc,
                    GiaCombo = combo.GiaCombo,
                    PhanTramTietKiem = combo.PhanTramTietKiem,
                    TrangThai = combo.TrangThai
                }).ToList(),

                DanhSachSanPham = danhSachSanPham.Select(sp => new KhuyenMaiViewModel
                {
                    Id = sp.Id,
                    TenSanPham = sp.TenSanPham,
                    GiaGoc = sp.Gia,
                    HinhAnh = sp.HinhAnh,
                    PhanTramGiam = sp.PhanTramGiam,
                    NgayBatDauKhuyenMai = sp.NgayBatDauKhuyenMai,
                    NgayKetThucKhuyenMai = sp.NgayKetThucKhuyenMai,
                    DangKhuyenMai = sp.DangKhuyenMai,
                    GiaSauGiam = sp.GiaSauGiam
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Combo(int? id)
        {
            var model = new ComboKhuyenMaiFormViewModel();

            if (id.HasValue)
            {
                var combo = await _context.Combos
                    .Include(x => x.ComboSanPhams)
                    .FirstOrDefaultAsync(x => x.Id == id.Value);

                if (combo == null)
                    return NotFound();

                model.Id = combo.Id;
                model.TenCombo = combo.TenCombo;
                model.MoTa = combo.MoTa;
                model.GiaCombo = combo.GiaCombo;
                model.TrangThai = combo.TrangThai;
                model.SanPhamIds = combo.ComboSanPhams.Select(x => x.SanPhamId).ToList();
            }

            await NapDanhSachSanPhamChoComboAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Combo(ComboKhuyenMaiFormViewModel model)
        {
            model.SanPhamIds = model.SanPhamIds.Distinct().ToList();

            if (model.SanPhamIds.Count < 2)
            {
                ModelState.AddModelError(nameof(model.SanPhamIds), "Combo phải có ít nhất 2 sản phẩm.");
            }

            var sanPhams = await _context.SanPhams
                .Where(x => model.SanPhamIds.Contains(x.Id) && x.TrangThai)
                .ToListAsync();

            if (sanPhams.Count != model.SanPhamIds.Count)
            {
                ModelState.AddModelError(nameof(model.SanPhamIds), "Danh sách sản phẩm trong combo không hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                await NapDanhSachSanPhamChoComboAsync(model);
                return View(model);
            }

            if (model.Id == 0)
            {
                var combo = new Combo
                {
                    TenCombo = model.TenCombo.Trim(),
                    MoTa = model.MoTa?.Trim(),
                    GiaCombo = model.GiaCombo,
                    TrangThai = model.TrangThai,
                    HinhAnh = sanPhams.FirstOrDefault()?.HinhAnh,
                    NgayTao = DateTime.Now,
                    ComboSanPhams = model.SanPhamIds.Select(id => new ComboSanPham
                    {
                        SanPhamId = id,
                        SoLuong = 1
                    }).ToList()
                };

                _context.Combos.Add(combo);
            }
            else
            {
                var combo = await _context.Combos
                    .Include(x => x.ComboSanPhams)
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (combo == null)
                    return NotFound();

                combo.TenCombo = model.TenCombo.Trim();
                combo.MoTa = model.MoTa?.Trim();
                combo.GiaCombo = model.GiaCombo;
                combo.TrangThai = model.TrangThai;
                combo.HinhAnh = sanPhams.FirstOrDefault()?.HinhAnh;

                _context.ComboSanPhams.RemoveRange(combo.ComboSanPhams);
                await _context.ComboSanPhams.AddRangeAsync(model.SanPhamIds.Select(id => new ComboSanPham
                {
                    ComboId = combo.Id,
                    SanPhamId = id,
                    SoLuong = 1
                }));
            }

            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = model.Id == 0
                ? "Đã tạo combo khuyến mãi."
                : "Đã cập nhật combo khuyến mãi.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThaiCombo(int id)
        {
            var combo = await _context.Combos.FindAsync(id);

            if (combo == null)
                return NotFound();

            combo.TrangThai = !combo.TrangThai;
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = combo.TrangThai
                ? "Đã bật combo."
                : "Đã tạm ngưng combo.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ChinhSua(int id)
        {
            var sanPham = await _dichVuSanPham.LayTheoIdAsync(id);
            if (sanPham == null)
                return NotFound();

            var model = new KhuyenMaiViewModel
            {
                Id = sanPham.Id,
                TenSanPham = sanPham.TenSanPham,
                GiaGoc = sanPham.Gia,
                HinhAnh = sanPham.HinhAnh,
                PhanTramGiam = sanPham.PhanTramGiam,
                NgayBatDauKhuyenMai = sanPham.NgayBatDauKhuyenMai,
                NgayKetThucKhuyenMai = sanPham.NgayKetThucKhuyenMai,
                DangKhuyenMai = sanPham.DangKhuyenMai,
                GiaSauGiam = sanPham.GiaSauGiam
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChinhSua(KhuyenMaiViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var sanPham = await _dichVuSanPham.LayTheoIdAsync(model.Id);
            if (sanPham == null)
                return NotFound();

            sanPham.PhanTramGiam = model.PhanTramGiam;
            sanPham.NgayBatDauKhuyenMai = model.NgayBatDauKhuyenMai;
            sanPham.NgayKetThucKhuyenMai = model.NgayKetThucKhuyenMai;

            await _dichVuSanPham.CapNhatAsync(sanPham);

            TempData["ThanhCong"] = "Cập nhật khuyến mãi thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> HuyKhuyenMai(int id)
        {
            var sanPham = await _dichVuSanPham.LayTheoIdAsync(id);
            if (sanPham == null)
                return NotFound();

            sanPham.PhanTramGiam = 0;
            sanPham.NgayBatDauKhuyenMai = null;
            sanPham.NgayKetThucKhuyenMai = null;

            await _dichVuSanPham.CapNhatAsync(sanPham);

            TempData["ThanhCong"] = "Đã hủy khuyến mãi.";
            return RedirectToAction(nameof(Index));
        }

        private async Task NapDanhSachSanPhamChoComboAsync(ComboKhuyenMaiFormViewModel model)
        {
            var sanPhamDaChon = model.SanPhamIds.ToHashSet();

            model.DanhSachSanPham = await _context.SanPhams
                .Where(x => x.TrangThai)
                .OrderBy(x => x.TenSanPham)
                .Select(x => new ComboSanPhamLuaChonViewModel
                {
                    Id = x.Id,
                    TenSanPham = x.TenSanPham,
                    Gia = x.Gia,
                    HinhAnh = x.HinhAnh,
                    DuocChon = sanPhamDaChon.Contains(x.Id)
                })
                .ToListAsync();
        }
    }
}
