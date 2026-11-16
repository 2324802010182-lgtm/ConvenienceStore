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
            var configuration = dichVu.GetRequiredService<IConfiguration>();
            var logger = dichVu.GetRequiredService<ILoggerFactory>().CreateLogger("KhoiTaoVaiTroVaAdmin");

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

            // 3. Tao tai khoan admin neu chua co. Mat khau phai duoc cap qua User Secrets / Environment.
            string emailAdmin = "admin@akdstore.com";

            var adminDaTonTai = await userManager.FindByEmailAsync(emailAdmin);

            if (adminDaTonTai == null)
            {
                var matKhauAdmin = configuration["BootstrapAdmin:Password"];
                if (string.IsNullOrWhiteSpace(matKhauAdmin))
                {
                    logger.LogWarning("Bo qua tao tai khoan admin mac dinh vi chua cau hinh BootstrapAdmin:Password trong User Secrets/Environment.");
                    return;
                }

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
                else
                {
                    foreach (var loi in ketQuaTaoAdmin.Errors)
                    {
                        logger.LogError("Khong tao duoc tai khoan admin: {ErrorCode} - {Description}", loi.Code, loi.Description);
                    }
                }
            }
        }
    }
}
