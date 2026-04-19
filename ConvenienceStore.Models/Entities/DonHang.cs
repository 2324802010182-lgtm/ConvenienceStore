using ConvenienceStore.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvenienceStore.Models.Entities
{
    public class DonHang
    {
        public int Id { get; set; }

        [Required]
        public string NguoiDungId { get; set; } = string.Empty;
        public NguoiDung? NguoiDung { get; set; }

        [Required(ErrorMessage = "Họ tên người nhận không được để trống")]
        [StringLength(200)]
        public string HoTenNguoiNhan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [StringLength(20)]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ nhận hàng không được để trống")]
        [StringLength(300)]
        public string DiaChiNhanHang { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTien { get; set; }

        public DateTime NgayDatHang { get; set; } = DateTime.Now;

        public TrangThaiDonHang TrangThai { get; set; } = TrangThaiDonHang.ChoXacNhan;

        public ICollection<ChiTietDonHang>? ChiTietDonHangs { get; set; }
    }
}