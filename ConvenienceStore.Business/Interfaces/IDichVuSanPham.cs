using ConvenienceStore.Models.Entities;

namespace ConvenienceStore.Business.Interfaces
{
    public interface IDichVuSanPham
    {
        Task<IEnumerable<SanPham>> LayTatCaKemDanhMucAsync();
        Task<IEnumerable<SanPham>> LaySanPhamDangBanAsync();
        Task<IEnumerable<SanPham>> LaySanPhamMoiAsync(int soLuong);
        Task<IEnumerable<SanPham>> LaySanPhamTheoDanhMucAsync(int danhMucId);
        Task<SanPham?> LayTheoIdAsync(int id);
        Task<SanPham?> LayTheoIdKemDanhMucAsync(int id);
        Task<IEnumerable<SanPham>> TimKiemSanPhamAsync(string tuKhoa);
        Task ThemAsync(SanPham sanPham);
        Task CapNhatAsync(SanPham sanPham);
        Task XoaAsync(int id);
        Task<IEnumerable<SanPham>> LaySanPhamFlashSaleAsync(int soLuong);
    }
}