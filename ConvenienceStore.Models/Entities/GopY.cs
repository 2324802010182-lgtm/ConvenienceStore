using ConvenienceStore.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ConvenienceStore.Models.Entities
{
    public class GopY
    {
        public int Id { get; set; }

        [Required]
        public string NguoiGuiId { get; set; } = string.Empty;
        public NguoiDung? NguoiGui { get; set; }

        [Required]
        public LoaiGopY LoaiGopY { get; set; }

        public int? SanPhamId { get; set; }
        public SanPham? SanPham { get; set; }

        public string? NhanVienId { get; set; }
        public NguoiDung? NhanVien { get; set; }

        [StringLength(300)]
        public string TieuDe { get; set; } = string.Empty;

        [StringLength(2000)]
        public string NoiDung { get; set; } = string.Empty;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public bool DaDong { get; set; } = false;

        public List<TinNhanGopY> TinNhans { get; set; } = new();
    }
}