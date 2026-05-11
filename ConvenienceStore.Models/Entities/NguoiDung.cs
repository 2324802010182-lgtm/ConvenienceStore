using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ConvenienceStore.Models.Entities
{
    public class NguoiDung : IdentityUser
    {
        [StringLength(200)]
        public string? HoTen { get; set; }

        [StringLength(500)]
        public string? DiaChi { get; set; }

        [StringLength(20)]
        public string? MaNhanVien { get; set; }

        public int DiemTichLuy { get; set; } = 0;
    }
}