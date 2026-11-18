using System.ComponentModel.DataAnnotations;

namespace ConvenienceStore.Web.ViewModels
{
    public class DoiMatKhauOtpViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Mã OTP")]
        public string Otp { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        [StringLength(100, MinimumLength = 12, ErrorMessage = "Mật khẩu phải có ít nhất 12 ký tự.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{12,}$", ErrorMessage = "Mật khẩu phải có chữ hoa, chữ thường, chữ số và ký tự đặc biệt.")]
        public string MatKhauMoi { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu nhập lại không khớp.")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string XacNhanMatKhau { get; set; } = string.Empty;
    }
}
