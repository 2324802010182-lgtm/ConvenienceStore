using ConvenienceStore.Models.Entities;

namespace ConvenienceStore.Business.Interfaces
{
    public interface IDichVuDanhMuc
    {
        Task<IEnumerable<DanhMuc>> LayTatCaAsync();
        Task<DanhMuc?> LayTheoIdAsync(int id);
        Task ThemAsync(DanhMuc danhMuc);
        Task CapNhatAsync(DanhMuc danhMuc);
        Task XoaAsync(int id);
    }
}