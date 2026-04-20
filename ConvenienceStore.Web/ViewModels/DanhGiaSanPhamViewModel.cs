using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ConvenienceStore.Web.ViewModels
{
    public class DanhGiaSanPhamViewModel
    {
        public int SanPhamId { get; set; }

        [Range(1, 5, ErrorMessage = "Vui lòng chọn số sao từ 1 đến 5")]
        public int SoSao { get; set; }

        [StringLength(1000, ErrorMessage = "Bình luận tối đa 1000 ký tự")]
        public string? BinhLuan { get; set; }

        public IFormFile? TepHinhAnh { get; set; }
    }
}