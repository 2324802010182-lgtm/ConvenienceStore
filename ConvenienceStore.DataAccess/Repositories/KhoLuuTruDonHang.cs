using ConvenienceStore.DataAccess.Interfaces;
using ConvenienceStore.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.DataAccess.Repositories
{
    public class KhoLuuTruDonHang : KhoLuuTruDungChung<DonHang>, IKhoLuuTruDonHang
    {
        private readonly ApplicationDbContext _context;

        public KhoLuuTruDonHang(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DonHang>> LayTatCaKemNguoiDungAsync()
        {
            return await _context.DonHangs
                .Include(x => x.NguoiDung)
                .OrderByDescending(x => x.NgayDatHang)
                .ToListAsync();
        }

        public async Task<DonHang?> LayChiTietDonHangAsync(int id)
        {
            return await _context.DonHangs
                .Include(x => x.NguoiDung)
                .Include(x => x.ChiTietDonHangs!)
                    .ThenInclude(ct => ct.SanPham)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<DonHang>> LayTheoNguoiDungAsync(string nguoiDungId)
        {
            return await _context.DonHangs
                .Include(x => x.NguoiDung)
                .Where(x => x.NguoiDungId == nguoiDungId)
                .OrderByDescending(x => x.NgayDatHang)
                .ToListAsync();
        }
    }
}