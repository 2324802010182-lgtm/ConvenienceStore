using System.ComponentModel.DataAnnotations.Schema;

namespace ConvenienceStore.Models.Entities
{
    public class ChiTietDonHang
    {
        public int Id { get; set; }

        public int DonHangId { get; set; }
        public DonHang? DonHang { get; set; }

        public int SanPhamId { get; set; }
        public SanPham? SanPham { get; set; }

        public int? ComboId { get; set; }
        public Combo? Combo { get; set; }

        public bool LaCombo { get; set; }

        public string? TenHienThi { get; set; }

        public string? HinhAnhHienThi { get; set; }

        public string? MoTaCombo { get; set; }

        public int SoLuong { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DonGia { get; set; }

        [NotMapped]
        public decimal ThanhTien => SoLuong * DonGia;
    }
}
