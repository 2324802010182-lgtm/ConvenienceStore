using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.DataAccess;
using ConvenienceStore.Models.Entities;
using ConvenienceStore.Models.Enums;
using ConvenienceStore.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.Business.Services
{
    public class DichVuDanhGiaSanPham : IDichVuDanhGiaSanPham
    {
        private readonly ApplicationDbContext _context;

        public DichVuDanhGiaSanPham(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DanhGiaSanPham>> LayTheoSanPhamAsync(int sanPhamId)
        {
            return await _context.DanhGiaSanPhams
                .Include(x => x.NguoiDung)
                .Where(x => x.SanPhamId == sanPhamId && !x.BiAn)
                .OrderByDescending(x => x.NgayDanhGia)
                .ToListAsync();
        }

        public async Task<List<DanhGiaSanPham>> LayTatCaAsync()
        {
            return await _context.DanhGiaSanPhams
                .Include(x => x.NguoiDung)
                .Include(x => x.SanPham)
                .OrderByDescending(x => x.NgayDanhGia)
                .ToListAsync();
        }

        public async Task<DanhGiaSanPham?> LayTheoIdAsync(int id)
        {
            return await _context.DanhGiaSanPhams
                .Include(x => x.NguoiDung)
                .Include(x => x.SanPham)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> KiemTraNguoiDungDaMuaSanPhamAsync(string nguoiDungId, int sanPhamId)
        {
            return await _context.ChiTietDonHangs
                .Include(x => x.DonHang)
                .AnyAsync(x =>
                    x.SanPhamId == sanPhamId &&
                    x.DonHang != null &&
                    x.DonHang.NguoiDungId == nguoiDungId &&
                    x.DonHang.TrangThai == TrangThaiDonHang.DaGiao);
        }

        public async Task<bool> KiemTraDaDanhGiaAsync(string nguoiDungId, int sanPhamId)
        {
            return await _context.DanhGiaSanPhams
                .AnyAsync(x => x.NguoiDungId == nguoiDungId && x.SanPhamId == sanPhamId);
        }

        public async Task ThemDanhGiaAsync(DanhGiaSanPham danhGia)
        {
            _context.DanhGiaSanPhams.Add(danhGia);
            await _context.SaveChangesAsync();
        }

        public async Task CapNhatAsync(DanhGiaSanPham danhGia)
        {
            _context.DanhGiaSanPhams.Update(danhGia);
            await _context.SaveChangesAsync();
        }

        public async Task<ThongKeDanhGiaViewModel> LayThongKeDanhGiaAsync(int sanPhamId)
        {
            var ds = await _context.DanhGiaSanPhams
                .Where(x => x.SanPhamId == sanPhamId && !x.BiAn)
                .ToListAsync();

            var tong = ds.Count;

            return new ThongKeDanhGiaViewModel
            {
                TongSoDanhGia = tong,
                DiemTrungBinh = tong == 0 ? 0 : Math.Round(ds.Average(x => x.SoSao), 1),
                SoLuong5Sao = ds.Count(x => x.SoSao == 5),
                SoLuong4Sao = ds.Count(x => x.SoSao == 4),
                SoLuong3Sao = ds.Count(x => x.SoSao == 3),
                SoLuong2Sao = ds.Count(x => x.SoSao == 2),
                SoLuong1Sao = ds.Count(x => x.SoSao == 1)
            };
        }
    }
}