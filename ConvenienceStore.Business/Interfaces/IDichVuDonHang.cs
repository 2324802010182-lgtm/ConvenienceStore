using ConvenienceStore.Models.Entities;
using ConvenienceStore.Models.Enums;

namespace ConvenienceStore.Business.Interfaces
{
    public interface IDichVuDonHang
    {
        Task<IEnumerable<DonHang>> LayTatCaAsync();
        Task<DonHang?> LayChiTietAsync(int id);
        Task<IEnumerable<DonHang>> LayDonHangTheoNguoiDungAsync(string nguoiDungId);
        Task CapNhatTrangThaiAsync(int id, TrangThaiDonHang trangThai);
        Task<int> TaoDonHangAsync(
            string nguoiDungId,
            int sanPhamId,
            int soLuong,
            string hoTenNguoiNhan,
            string soDienThoai,
            string diaChiNhanHang);
        Task<int> TaoDonHangTuGioAsync(
            string nguoiDungId,
            List<SanPhamDatHang> danhSachSanPham,
            string hoTenNguoiNhan,
            string soDienThoai,
            string diaChiNhanHang);
    }
}