using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvenienceStore.Models.Entities
{
    public class Combo
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string TenCombo { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? MoTa { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaCombo { get; set; }

        public string? HinhAnh { get; set; }

        public bool TrangThai { get; set; } = true;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public ICollection<ComboSanPham> ComboSanPhams { get; set; } = new List<ComboSanPham>();

        [NotMapped]
        public decimal GiaGoc => ComboSanPhams.Sum(x => (x.SanPham?.Gia ?? 0) * x.SoLuong);

        [NotMapped]
        public int PhanTramTietKiem
        {
            get
            {
                if (GiaGoc <= 0 || GiaCombo >= GiaGoc)
                    return 0;

                return (int)Math.Round((GiaGoc - GiaCombo) / GiaGoc * 100m);
            }
        }
    }
}
