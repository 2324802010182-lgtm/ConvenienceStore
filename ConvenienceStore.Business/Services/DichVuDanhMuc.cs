using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.DataAccess.UnitOfWork;
using ConvenienceStore.Models.Entities;

namespace ConvenienceStore.Business.Services
{
    public class DichVuDanhMuc : IDichVuDanhMuc
    {
        private readonly IDonViCongViec _donViCongViec;

        public DichVuDanhMuc(IDonViCongViec donViCongViec)
        {
            _donViCongViec = donViCongViec;
        }

        public async Task<IEnumerable<DanhMuc>> LayTatCaAsync()
        {
            return await _donViCongViec.DanhMucs.LayTatCaAsync();
        }

        public async Task<DanhMuc?> LayTheoIdAsync(int id)
        {
            return await _donViCongViec.DanhMucs.LayTheoIdAsync(id);
        }

        public async Task ThemAsync(DanhMuc danhMuc)
        {
            await _donViCongViec.DanhMucs.ThemAsync(danhMuc);
            await _donViCongViec.LuuThayDoiAsync();
        }

        public async Task CapNhatAsync(DanhMuc danhMuc)
        {
            _donViCongViec.DanhMucs.Sua(danhMuc);
            await _donViCongViec.LuuThayDoiAsync();
        }

        public async Task XoaAsync(int id)
        {
            var danhMuc = await _donViCongViec.DanhMucs.LayTheoIdAsync(id);
            if (danhMuc != null)
            {
                _donViCongViec.DanhMucs.Xoa(danhMuc);
                await _donViCongViec.LuuThayDoiAsync();
            }
        }
    }
}