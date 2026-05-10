using System.ComponentModel.DataAnnotations;

namespace ConvenienceStore.Models.Entities
{
    public class LichSuDiem
    {
        [Key]
        public int Id { get; set; }

        public string NguoiDungId { get; set; } = string.Empty;

        public int? DonHangId { get; set; }

        public int SoDiem { get; set; }

        public string LoaiGiaoDich { get; set; } = string.Empty;
        // CONG_DIEM hoặc DOI_DIEM

        public decimal SoTienTuongUng { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public string? GhiChu { get; set; }
    }
}
