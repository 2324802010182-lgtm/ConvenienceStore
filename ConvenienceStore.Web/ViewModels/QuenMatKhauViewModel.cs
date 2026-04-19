using System.ComponentModel.DataAnnotations;

namespace ConvenienceStore.Web.ViewModels
{
    public class QuenMatKhauViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;
    }
}