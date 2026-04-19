using ConvenienceStore.Models.Entities;

namespace ConvenienceStore.DataAccess.Interfaces
{
    public interface IKhoLuuTruDonHang : IKhoLuuTruDungChung<DonHang>
    {
        Task<IEnumerable<DonHang>> LayTatCaKemNguoiDungAsync();
        Task<DonHang?> LayChiTietDonHangAsync(int id);
        Task<IEnumerable<DonHang>> LayTheoNguoiDungAsync(string nguoiDungId);
    }
}