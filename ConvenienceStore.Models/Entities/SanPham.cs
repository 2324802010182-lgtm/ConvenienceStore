using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvenienceStore.Models.Entities
{
    public class SanPham
    {
        public int Id { get; set; }

        [Required]
        public string TenSanPham { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Gia { get; set; }

        public int SoLuongTon { get; set; }

        public string? HinhAnh { get; set; }

        public bool TrangThai { get; set; }

        public int DanhMucId { get; set; }
        public DanhMuc? DanhMuc { get; set; }

        // ===== KHUYẾN MÃI =====
        [Range(0, 100)]
        public int PhanTramGiam { get; set; } = 0;

        public DateTime? NgayBatDauKhuyenMai { get; set; }

        public DateTime? NgayKetThucKhuyenMai { get; set; }

        [NotMapped]
        public bool DangKhuyenMai
        {
            get
            {
                var bayGio = DateTime.Now;
                return PhanTramGiam > 0
                    && NgayBatDauKhuyenMai.HasValue
                    && NgayKetThucKhuyenMai.HasValue
                    && bayGio >= NgayBatDauKhuyenMai.Value
                    && bayGio <= NgayKetThucKhuyenMai.Value;
            }
        }

        [NotMapped]
        public decimal GiaSauGiam
        {
            get
            {
                if (!DangKhuyenMai) return Gia;
                return Gia - (Gia * PhanTramGiam / 100m);
            }
        }
    }
}