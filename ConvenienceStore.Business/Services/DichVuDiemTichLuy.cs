using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.DataAccess;
using ConvenienceStore.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.Business.Services
{
    public class DichVuDiemTichLuy : IDichVuDiemTichLuy
    {
        private readonly ApplicationDbContext _context;

        public DichVuDiemTichLuy(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TinhDiemDuocCong(decimal soTienThanhToan)
        {
            return (int)(soTienThanhToan / 10000);
        }

        public decimal TinhTienGiamTuDiem(int soDiem)
        {
            return soDiem * 1000;
        }

        public async Task<int> LayDiemHienCoAsync(string nguoiDungId)
        {
            var nguoiDung = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == nguoiDungId);

            return nguoiDung?.DiemTichLuy ?? 0;
        }

        public async Task<List<LichSuDiem>> LayLichSuDiemAsync(string nguoiDungId)
        {
            return await _context.LichSuDiems
                .Where(x => x.NguoiDungId == nguoiDungId)
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();
        }

        public async Task CongDiemSauThanhToanAsync(
            string nguoiDungId,
            int donHangId,
            decimal soTienThanhToan)
        {
            var nguoiDung = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == nguoiDungId);

            if (nguoiDung == null)
                return;

            int diemCong = TinhDiemDuocCong(soTienThanhToan);

            if (diemCong <= 0)
                return;

            nguoiDung.DiemTichLuy += diemCong;

            var lichSu = new LichSuDiem
            {
                NguoiDungId = nguoiDungId,
                DonHangId = donHangId,
                SoDiem = diemCong,
                LoaiGiaoDich = "CONG_DIEM",
                SoTienTuongUng = soTienThanhToan,
                NgayTao = DateTime.Now,
                GhiChu = $"Cộng {diemCong} điểm từ đơn hàng #{donHangId}"
            };

            _context.LichSuDiems.Add(lichSu);

            await _context.SaveChangesAsync();
        }

        public async Task<decimal> DoiDiemAsync(
            string nguoiDungId,
            int donHangId,
            int soDiemMuonDoi)
        {
            if (soDiemMuonDoi <= 0)
                return 0;

            var nguoiDung = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == nguoiDungId);

            if (nguoiDung == null)
                throw new Exception("Không tìm thấy người dùng.");

            if (nguoiDung.DiemTichLuy < soDiemMuonDoi)
                throw new Exception("Số điểm tích lũy không đủ để quy đổi.");

            decimal soTienGiam = TinhTienGiamTuDiem(soDiemMuonDoi);

            nguoiDung.DiemTichLuy -= soDiemMuonDoi;

            var lichSu = new LichSuDiem
            {
                NguoiDungId = nguoiDungId,
                DonHangId = donHangId,
                SoDiem = -soDiemMuonDoi,
                LoaiGiaoDich = "DOI_DIEM",
                SoTienTuongUng = soTienGiam,
                NgayTao = DateTime.Now,
                GhiChu = $"Đổi {soDiemMuonDoi} điểm giảm {soTienGiam:N0}đ cho đơn hàng #{donHangId}"
            };

            _context.LichSuDiems.Add(lichSu);

            await _context.SaveChangesAsync();

            return soTienGiam;
        }
    }
}