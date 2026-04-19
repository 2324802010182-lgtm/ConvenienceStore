using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.DataAccess.UnitOfWork;
using ConvenienceStore.Models.Entities;

namespace ConvenienceStore.Business.Services
{
    public class DichVuSanPham : IDichVuSanPham
    {
        private readonly IDonViCongViec _donViCongViec;

        public DichVuSanPham(IDonViCongViec donViCongViec)
        {
            _donViCongViec = donViCongViec;
        }

        public async Task<IEnumerable<SanPham>> LayTatCaKemDanhMucAsync()
        {
            return await _donViCongViec.SanPhams.LayTatCaKemDanhMucAsync();
        }

        public async Task<IEnumerable<SanPham>> LaySanPhamDangBanAsync()
        {
            return await _donViCongViec.SanPhams.LaySanPhamDangBanAsync();
        }

        public async Task<IEnumerable<SanPham>> LaySanPhamMoiAsync(int soLuong)
        {
            return await _donViCongViec.SanPhams.LaySanPhamMoiAsync(soLuong);
        }

        public async Task<IEnumerable<SanPham>> LaySanPhamTheoDanhMucAsync(int danhMucId)
        {
            return await _donViCongViec.SanPhams.LaySanPhamTheoDanhMucAsync(danhMucId);
        }

        public async Task<SanPham?> LayTheoIdAsync(int id)
        {
            return await _donViCongViec.SanPhams.LayTheoIdAsync(id);
        }

        public async Task<SanPham?> LayTheoIdKemDanhMucAsync(int id)
        {
            return await _donViCongViec.SanPhams.LayTheoIdKemDanhMucAsync(id);
        }
        public async Task<IEnumerable<SanPham>> LaySanPhamFlashSaleAsync(int soLuong)
        {
            return await _donViCongViec.SanPhams.LaySanPhamFlashSaleAsync(soLuong);
        }
        public async Task<IEnumerable<SanPham>> TimKiemSanPhamAsync(string tuKhoa)
        {
            if (string.IsNullOrWhiteSpace(tuKhoa))
                return await _donViCongViec.SanPhams.LaySanPhamDangBanAsync();

            return await _donViCongViec.SanPhams.TimKiemSanPhamAsync(tuKhoa);
        }

        public async Task ThemAsync(SanPham sanPham)
        {
            await _donViCongViec.SanPhams.ThemAsync(sanPham);
            await _donViCongViec.LuuThayDoiAsync();
        }

        public async Task CapNhatAsync(SanPham sanPham)
        {
            _donViCongViec.SanPhams.Sua(sanPham);
            await _donViCongViec.LuuThayDoiAsync();
        }

        public async Task XoaAsync(int id)
        {
            var sanPham = await _donViCongViec.SanPhams.LayTheoIdAsync(id);
            if (sanPham != null)
            {
                _donViCongViec.SanPhams.Xoa(sanPham);
                await _donViCongViec.LuuThayDoiAsync();
            }
        }
    }
}