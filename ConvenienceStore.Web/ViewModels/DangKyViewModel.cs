using System.ComponentModel.DataAnnotations;

namespace ConvenienceStore.Web.ViewModels
{
    public class DangKyViewModel
    {
        [Required(ErrorMessage = "Ho ten khong duoc de trong")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email khong duoc de trong")]
        [EmailAddress(ErrorMessage = "Email khong hop le")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mat khau khong duoc de trong")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 12, ErrorMessage = "Mật khẩu phải có ít nhất 12 ký tự.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{12,}$", ErrorMessage = "Mật khẩu phải có chữ hoa, chữ thường, chữ số và ký tự đặc biệt.")]
        public string MatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xac nhan mat khau khong duoc de trong")]
        [DataType(DataType.Password)]
        [Compare("MatKhau", ErrorMessage = "Xac nhan mat khau khong khop")]
        public string XacNhanMatKhau { get; set; } = string.Empty;

        public string? DiaChi { get; set; }
    }
}
