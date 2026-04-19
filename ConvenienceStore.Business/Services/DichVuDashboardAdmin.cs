using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.DataAccess;
using ConvenienceStore.Models.Enums;
using ConvenienceStore.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.Business.Services
{
    public class DichVuDashboardAdmin : IDichVuDashboardAdmin
    {
        private readonly ApplicationDbContext _context;

        public DichVuDashboardAdmin(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardAdminViewModel> LayDuLieuTongQuanAsync()
        {
            var tongSanPham = await _context.SanPhams.CountAsync();
            var tongDanhMuc = await _context.DanhMucs.CountAsync();
            var tongDonHang = await _context.DonHangs.CountAsync();
            var tongNguoiDung = await _context.Users.CountAsync();

            var tongDoanhThu = await _context.DonHangs
                .Where(x => x.TrangThai == TrangThaiDonHang.DaGiao)
                .SumAsync(x => (decimal?)x.TongTien) ?? 0;

            var donChoXacNhan = await _context.DonHangs
                .CountAsync(x => x.TrangThai == TrangThaiDonHang.ChoXacNhan);

            var sanPhamSapHetHang = await _context.SanPhams
                .CountAsync(x => x.SoLuongTon > 0 && x.SoLuongTon <= 5);

            var hoatDongGanDay = await _context.DonHangs
                .OrderByDescending(x => x.NgayDatHang)
                .Take(4)
                .Select(x => $"Đơn hàng #{x.Id} - {x.HoTenNguoiNhan} - {x.NgayDatHang:dd/MM/yyyy HH:mm}")
                .ToListAsync();

            return new DashboardAdminViewModel
            {
                TongSanPham = tongSanPham,
                TongDanhMuc = tongDanhMuc,
                TongDonHang = tongDonHang,
                TongNguoiDung = tongNguoiDung,
                TongDoanhThu = tongDoanhThu,
                DonChoXacNhan = donChoXacNhan,
                SanPhamSapHetHang = sanPhamSapHetHang,
                HoatDongGanDay = hoatDongGanDay
            };
        }
    }
}