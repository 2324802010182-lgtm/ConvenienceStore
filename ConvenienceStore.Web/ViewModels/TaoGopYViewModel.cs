using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ConvenienceStore.Web.ViewModels
{
    public class TaoGopYViewModel
    {
        [Required]
        public int LoaiGopY { get; set; }

        public int? SanPhamId { get; set; }

        public string? NhanVienId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        public string TieuDe { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string NoiDung { get; set; } = string.Empty;

        public List<SelectListItem> DanhSachSanPham { get; set; } = new();
        public List<SelectListItem> DanhSachNhanVien { get; set; } = new();
    }
}