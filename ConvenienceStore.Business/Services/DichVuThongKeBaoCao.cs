using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.DataAccess;
using ConvenienceStore.Models.Enums;
using ConvenienceStore.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.Business.Services
{
    public class DichVuThongKeBaoCao : IDichVuThongKeBaoCao
    {
        private readonly ApplicationDbContext _context;

        public DichVuThongKeBaoCao(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ThongKeBaoCaoViewModel> LayDuLieuBaoCaoAsync()
        {
            var donDaGiao = await _context.DonHangs
                .Where(x => x.TrangThai == TrangThaiDonHang.DaGiao)
                .ToListAsync();

            var chiTietDaGiao = await _context.ChiTietDonHangs
                .Include(x => x.SanPham)
                .Include(x => x.DonHang)
                .Where(x => x.DonHang != null && x.DonHang.TrangThai == TrangThaiDonHang.DaGiao)
                .ToListAsync();

            var model = new ThongKeBaoCaoViewModel();

            model.TongDoanhThu = donDaGiao.Sum(x => x.TongTien);
            model.TongDonDaGiao = donDaGiao.Count;

            var doanhThuTheoNgay = donDaGiao
                .GroupBy(x => x.NgayDatHang.Date)
                .OrderBy(x => x.Key)
                .ToList();

            model.NhanDoanhThuTheoNgay = doanhThuTheoNgay
                .Select(x => x.Key.ToString("dd/MM/yyyy"))
                .ToList();

            model.DuLieuDoanhThuTheoNgay = doanhThuTheoNgay
                .Select(x => x.Sum(y => y.TongTien))
                .ToList();

            var doanhThuTheoTuan = donDaGiao
                .GroupBy(x => new
                {
                    Nam = x.NgayDatHang.Year,
                    Tuan = ((x.NgayDatHang.DayOfYear - 1) / 7) + 1
                })
                .OrderBy(x => x.Key.Nam)
                .ThenBy(x => x.Key.Tuan)
                .ToList();

            model.NhanDoanhThuTheoTuan = doanhThuTheoTuan
                .Select(x => $"Tuần {x.Key.Tuan}/{x.Key.Nam}")
                .ToList();

            model.DuLieuDoanhThuTheoTuan = doanhThuTheoTuan
                .Select(x => x.Sum(y => y.TongTien))
                .ToList();

            var doanhThuTheoThang = donDaGiao
                .GroupBy(x => new { x.NgayDatHang.Year, x.NgayDatHang.Month })
                .OrderBy(x => x.Key.Year)
                .ThenBy(x => x.Key.Month)
                .ToList();

            model.NhanDoanhThuTheoThang = doanhThuTheoThang
                .Select(x => $"{x.Key.Month:D2}/{x.Key.Year}")
                .ToList();

            model.DuLieuDoanhThuTheoThang = doanhThuTheoThang
                .Select(x => x.Sum(y => y.TongTien))
                .ToList();

            var doanhThuTheoQuy = donDaGiao
                .GroupBy(x => new
                {
                    x.NgayDatHang.Year,
                    Quy = ((x.NgayDatHang.Month - 1) / 3) + 1
                })
                .OrderBy(x => x.Key.Year)
                .ThenBy(x => x.Key.Quy)
                .ToList();

            model.NhanDoanhThuTheoQuy = doanhThuTheoQuy
                .Select(x => $"Q{x.Key.Quy}/{x.Key.Year}")
                .ToList();

            model.DuLieuDoanhThuTheoQuy = doanhThuTheoQuy
                .Select(x => x.Sum(y => y.TongTien))
                .ToList();

            var doanhThuTheoNam = donDaGiao
                .GroupBy(x => x.NgayDatHang.Year)
                .OrderBy(x => x.Key)
                .ToList();

            model.NhanDoanhThuTheoNam = doanhThuTheoNam
                .Select(x => x.Key.ToString())
                .ToList();

            model.DuLieuDoanhThuTheoNam = doanhThuTheoNam
                .Select(x => x.Sum(y => y.TongTien))
                .ToList();

            var sanPhamBanChay = chiTietDaGiao
                .Where(x => x.SanPham != null)
                .GroupBy(x => x.SanPham!.TenSanPham)
                .Select(x => new
                {
                    TenSanPham = x.Key,
                    TongSoLuong = x.Sum(y => y.SoLuong)
                })
                .OrderByDescending(x => x.TongSoLuong)
                .Take(5)
                .ToList();

            model.NhanSanPhamBanChay = sanPhamBanChay
                .Select(x => x.TenSanPham)
                .ToList();

            model.DuLieuSanPhamBanChay = sanPhamBanChay
                .Select(x => x.TongSoLuong)
                .ToList();

            var sanPhamBanCham = chiTietDaGiao
                .Where(x => x.SanPham != null)
                .GroupBy(x => x.SanPham!.TenSanPham)
                .Select(x => new
                {
                    TenSanPham = x.Key,
                    TongSoLuong = x.Sum(y => y.SoLuong)
                })
                .OrderBy(x => x.TongSoLuong)
                .Take(5)
                .ToList();

            model.NhanSanPhamBanCham = sanPhamBanCham
                .Select(x => x.TenSanPham)
                .ToList();

            model.DuLieuSanPhamBanCham = sanPhamBanCham
                .Select(x => x.TongSoLuong)
                .ToList();

            return model;
        }
    }
}