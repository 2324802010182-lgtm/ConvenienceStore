using System.ComponentModel.DataAnnotations;

namespace ConvenienceStore.Web.ViewModels
{
    public class DatLaiMatKhauViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required(ErrorMessage = "Mat khau moi khong duoc de trong")]
        [DataType(DataType.Password)]
        public string MatKhauMoi { get; set; }

        [Required(ErrorMessage = "Xac nhan mat khau khong duoc de trong")]
        [DataType(DataType.Password)]
        [Compare("MatKhauMoi", ErrorMessage = "Xac nhan mat khau khong khop")]
        public string XacNhanMatKhau { get; set; }
    }
}