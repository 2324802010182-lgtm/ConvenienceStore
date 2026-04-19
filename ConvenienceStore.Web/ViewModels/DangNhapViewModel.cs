using System.ComponentModel.DataAnnotations;

namespace ConvenienceStore.Web.ViewModels
{
    public class DangNhapViewModel
    {
        [Required(ErrorMessage = "Email khong duoc de trong")]
        [EmailAddress(ErrorMessage = "Email khong hop le")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mat khau khong duoc de trong")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }

        public bool GhiNhoDangNhap { get; set; }
    }
}