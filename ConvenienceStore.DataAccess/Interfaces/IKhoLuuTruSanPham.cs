using ConvenienceStore.Models.Entities;

namespace ConvenienceStore.DataAccess.Interfaces
{
    public interface IKhoLuuTruSanPham : IKhoLuuTruDungChung<SanPham>
    {
        Task<IEnumerable<SanPham>> LayTatCaKemDanhMucAsync();
        Task<SanPham?> LayTheoIdKemDanhMucAsync(int id);
        Task<IEnumerable<SanPham>> LaySanPhamDangBanAsync();
        Task<IEnumerable<SanPham>> LaySanPhamMoiAsync(int soLuong);
        Task<IEnumerable<SanPham>> LaySanPhamTheoDanhMucAsync(int danhMucId);
        Task<IEnumerable<SanPham>> TimKiemSanPhamAsync(string tuKhoa);
        Task<IEnumerable<SanPham>> LaySanPhamFlashSaleAsync(int soLuong);
    }
}