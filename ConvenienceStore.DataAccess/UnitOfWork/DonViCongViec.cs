using ConvenienceStore.DataAccess.Interfaces;
using ConvenienceStore.DataAccess.Repositories;
using ConvenienceStore.Models.Entities;

namespace ConvenienceStore.DataAccess.UnitOfWork
{
    public class DonViCongViec : IDonViCongViec
    {
        private readonly ApplicationDbContext _context;

        public IKhoLuuTruDungChung<DanhMuc> DanhMucs { get; }
        public IKhoLuuTruSanPham SanPhams { get; }
        public IKhoLuuTruDonHang DonHangs { get; }
        public IKhoLuuTruDungChung<ChiTietDonHang> ChiTietDonHangs { get; }

        public DonViCongViec(ApplicationDbContext context)
        {
            _context = context;
            DanhMucs = new KhoLuuTruDungChung<DanhMuc>(_context);
            SanPhams = new KhoLuuTruSanPham(_context);
            DonHangs = new KhoLuuTruDonHang(_context);
            ChiTietDonHangs = new KhoLuuTruDungChung<ChiTietDonHang>(_context);
        }

        public async Task<int> LuuThayDoiAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}