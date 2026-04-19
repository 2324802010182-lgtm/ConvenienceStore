using System.ComponentModel.DataAnnotations;

namespace ConvenienceStore.Web.ViewModels
{
    public class QuenMatKhauViewModel
    {
        [Required(ErrorMessage = "Email khong duoc de trong")]
        [EmailAddress(ErrorMessage = "Email khong hop le")]
        public string Email { get; set; }
    }
}