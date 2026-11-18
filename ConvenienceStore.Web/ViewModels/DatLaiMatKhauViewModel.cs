using System.ComponentModel.DataAnnotations;

namespace ConvenienceStore.Web.ViewModels
{
    public class DatLaiMatKhauViewModel
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        [StringLength(100, MinimumLength = 12, ErrorMessage = "Mật khẩu phải có ít nhất 12 ký tự.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{12,}$", ErrorMessage = "Mật khẩu phải có chữ hoa, chữ thường, chữ số và ký tự đặc biệt.")]
        public string MatKhauMoi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("MatKhauMoi", ErrorMessage = "Xác nhận mật khẩu không khớp")]
        public string XacNhanMatKhau { get; set; } = string.Empty;
    }
}
