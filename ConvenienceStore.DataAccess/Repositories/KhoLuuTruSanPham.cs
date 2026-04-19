using ConvenienceStore.DataAccess.Interfaces;
using ConvenienceStore.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.DataAccess.Repositories
{
    public class KhoLuuTruSanPham : KhoLuuTruDungChung<SanPham>, IKhoLuuTruSanPham
    {
        private readonly ApplicationDbContext _context;

        public KhoLuuTruSanPham(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SanPham>> LayTatCaKemDanhMucAsync()
        {
            return await _context.SanPhams
                .Include(sp => sp.DanhMuc)
                .ToListAsync();
        }

        public async Task<SanPham?> LayTheoIdKemDanhMucAsync(int id)
        {
            return await _context.SanPhams
                .Include(sp => sp.DanhMuc)
                .FirstOrDefaultAsync(sp => sp.Id == id);
        }

        public async Task<IEnumerable<SanPham>> LaySanPhamDangBanAsync()
        {
            return await _context.SanPhams
                .Include(sp => sp.DanhMuc)
                .Where(sp => sp.TrangThai && sp.SoLuongTon > 0)
                .OrderByDescending(sp => sp.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<SanPham>> LaySanPhamMoiAsync(int soLuong)
        {
            return await _context.SanPhams
                .Include(sp => sp.DanhMuc)
                .Where(sp => sp.TrangThai && sp.SoLuongTon > 0)
                .OrderByDescending(sp => sp.Id)
                .Take(soLuong)
                .ToListAsync();
        }
        public async Task<IEnumerable<SanPham>> LaySanPhamFlashSaleAsync(int soLuong)
        {
            var bayGio = DateTime.Now;

            return await _context.SanPhams
                .Include(sp => sp.DanhMuc)
                .Where(sp => sp.TrangThai
                             && sp.SoLuongTon > 0
                             && sp.PhanTramGiam > 0
                             && sp.NgayBatDauKhuyenMai.HasValue
                             && sp.NgayKetThucKhuyenMai.HasValue
                             && bayGio >= sp.NgayBatDauKhuyenMai.Value
                             && bayGio <= sp.NgayKetThucKhuyenMai.Value)
                .OrderByDescending(sp => sp.PhanTramGiam)
                .ThenByDescending(sp => sp.Id)
                .Take(soLuong)
                .ToListAsync();
        }

        public async Task<IEnumerable<SanPham>> LaySanPhamTheoDanhMucAsync(int danhMucId)
        {
            return await _context.SanPhams
                .Include(sp => sp.DanhMuc)
                .Where(sp => sp.TrangThai && sp.SoLuongTon > 0 && sp.DanhMucId == danhMucId)
                .OrderByDescending(sp => sp.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<SanPham>> TimKiemSanPhamAsync(string tuKhoa)
        {
            tuKhoa = tuKhoa?.Trim().ToLower() ?? "";

            return await _context.SanPhams
                .Include(sp => sp.DanhMuc)
                .Where(sp => sp.TrangThai
                             && sp.SoLuongTon > 0
                             && sp.TenSanPham.ToLower().Contains(tuKhoa))
                .OrderBy(sp => sp.TenSanPham)
                .ToListAsync();
        }
    }
}