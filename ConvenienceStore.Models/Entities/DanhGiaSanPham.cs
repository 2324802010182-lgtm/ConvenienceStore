using System.ComponentModel.DataAnnotations;

namespace ConvenienceStore.Models.Entities
{
    public class DanhGiaSanPham
    {
        public int Id { get; set; }

        [Required]
        public int SanPhamId { get; set; }
        public SanPham? SanPham { get; set; }

        [Required]
        public string NguoiDungId { get; set; } = string.Empty;
        public NguoiDung? NguoiDung { get; set; }

        [Range(1, 5)]
        public int SoSao { get; set; }

        [StringLength(1000)]
        public string? BinhLuan { get; set; }

        public string? HinhAnhDanhGia { get; set; }

        public DateTime NgayDanhGia { get; set; } = DateTime.Now;

        public bool BiAn { get; set; } = false;
    }
}