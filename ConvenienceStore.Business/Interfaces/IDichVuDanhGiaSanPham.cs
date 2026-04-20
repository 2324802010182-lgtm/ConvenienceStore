using ConvenienceStore.Models.Entities;
using ConvenienceStore.Models.ViewModels;

namespace ConvenienceStore.Business.Interfaces
{
    public interface IDichVuDanhGiaSanPham
    {
        Task<List<DanhGiaSanPham>> LayTheoSanPhamAsync(int sanPhamId);
        Task<List<DanhGiaSanPham>> LayTatCaAsync();
        Task<DanhGiaSanPham?> LayTheoIdAsync(int id);
        Task<bool> KiemTraNguoiDungDaMuaSanPhamAsync(string nguoiDungId, int sanPhamId);
        Task<bool> KiemTraDaDanhGiaAsync(string nguoiDungId, int sanPhamId);
        Task ThemDanhGiaAsync(DanhGiaSanPham danhGia);
        Task CapNhatAsync(DanhGiaSanPham danhGia);
        Task<ThongKeDanhGiaViewModel> LayThongKeDanhGiaAsync(int sanPhamId);
    }
}