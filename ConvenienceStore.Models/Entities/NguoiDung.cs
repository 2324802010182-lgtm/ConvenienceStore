using Microsoft.AspNetCore.Identity;

namespace ConvenienceStore.Models.Entities
{
    public class NguoiDung : IdentityUser
    {
        public string? HoTen { get; set; }
        public string? DiaChi { get; set; }
    }
}