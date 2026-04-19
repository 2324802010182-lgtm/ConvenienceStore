using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ConvenienceStore.Web.ViewModels
{
    public class SanPhamViewModel
    {
        public int Id { get; set; }

        [Required]
        public string TenSanPham { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        [Required]
        public decimal Gia { get; set; }

        [Required]
        public int SoLuongTon { get; set; }

        public string? HinhAnh { get; set; }

        public bool TrangThai { get; set; }

        [Required]
        public int DanhMucId { get; set; }

        public IEnumerable<SelectListItem> DanhSachDanhMuc { get; set; } = new List<SelectListItem>();
    }
}