using ConvenienceStore.Models.Entities;

namespace ConvenienceStore.Business.Interfaces
{
    public interface IDichVuDiemTichLuy
    {
        int TinhDiemDuocCong(decimal soTienThanhToan);

        decimal TinhTienGiamTuDiem(int soDiem);

        Task<int> LayDiemHienCoAsync(string nguoiDungId);

        Task<List<LichSuDiem>> LayLichSuDiemAsync(string nguoiDungId);

        Task CongDiemSauThanhToanAsync(
            string nguoiDungId,
            int donHangId,
            decimal soTienThanhToan
        );

        Task<decimal> DoiDiemAsync(
            string nguoiDungId,
            int donHangId,
            int soDiemMuonDoi
        );
    }
}