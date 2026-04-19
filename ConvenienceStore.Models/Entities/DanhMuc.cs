using System.ComponentModel.DataAnnotations;

namespace ConvenienceStore.Models.Entities
{
    public class DanhMuc
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ten danh muc khong duoc de trong")]
        [StringLength(100)]
        public string TenDanhMuc { get; set; }

        public string? MoTa { get; set; }

        public bool TrangThai { get; set; } = true;

        public ICollection<SanPham>? DanhSachSanPham { get; set; }
    }
}