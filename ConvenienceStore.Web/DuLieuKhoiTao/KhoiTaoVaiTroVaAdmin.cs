using ConvenienceStore.Models.Entities;
using ConvenienceStore.Models.HangSo;
using Microsoft.AspNetCore.Identity;

namespace ConvenienceStore.Web.DuLieuKhoiTao
{
    public static class KhoiTaoVaiTroVaAdmin
    {
        public static async Task KhoiTaoAsync(IServiceProvider dichVu)
        {
            var roleManager = dichVu.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = dichVu.GetRequiredService<UserManager<NguoiDung>>();

            // 1. Tao vai tro Admin neu chua co
            if (!await roleManager.RoleExistsAsync(VaiTro.Admin))
            {
                await roleManager.CreateAsync(new IdentityRole(VaiTro.Admin));
            }

            // 2. Tao vai tro KhachHang neu chua co
            if (!await roleManager.RoleExistsAsync(VaiTro.KhachHang))
            {
                await roleManager.CreateAsync(new IdentityRole(VaiTro.KhachHang));
            }

            // 3. Tao tai khoan admin mac dinh neu chua co
            string emailAdmin = "admin@akdstore.com";
            string matKhauAdmin = "Admin@123";

            var adminDaTonTai = await userManager.FindByEmailAsync(emailAdmin);

            if (adminDaTonTai == null)
            {
                var admin = new NguoiDung
                {
                    UserName = emailAdmin,
                    Email = emailAdmin,
                    HoTen = "Quản trị hệ thống",
                    EmailConfirmed = true
                };

                var ketQuaTaoAdmin = await userManager.CreateAsync(admin, matKhauAdmin);

                if (ketQuaTaoAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, VaiTro.Admin);
                }
            }
        }
    }
}