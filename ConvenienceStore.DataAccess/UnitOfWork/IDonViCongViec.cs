using ConvenienceStore.DataAccess.Interfaces;
using ConvenienceStore.Models.Entities;

namespace ConvenienceStore.DataAccess.UnitOfWork
{
    public interface IDonViCongViec : IDisposable
    {
        IKhoLuuTruDungChung<DanhMuc> DanhMucs { get; }
        IKhoLuuTruSanPham SanPhams { get; }
        IKhoLuuTruDonHang DonHangs { get; }
        IKhoLuuTruDungChung<ChiTietDonHang> ChiTietDonHangs { get; }

        Task<int> LuuThayDoiAsync();
    }
}