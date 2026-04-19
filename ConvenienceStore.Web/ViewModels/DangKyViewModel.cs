using System.ComponentModel.DataAnnotations;

namespace ConvenienceStore.Web.ViewModels
{
    public class DangKyViewModel
    {
        [Required(ErrorMessage = "Ho ten khong duoc de trong")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Email khong duoc de trong")]
        [EmailAddress(ErrorMessage = "Email khong hop le")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mat khau khong duoc de trong")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Xac nhan mat khau khong duoc de trong")]
        [DataType(DataType.Password)]
        [Compare("MatKhau", ErrorMessage = "Xac nhan mat khau khong khop")]
        public string XacNhanMatKhau { get; set; }

        public string? DiaChi { get; set; }
    }
}