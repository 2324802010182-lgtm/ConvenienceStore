using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.Business.Services;
using ConvenienceStore.DataAccess;
using ConvenienceStore.DataAccess.UnitOfWork;
using ConvenienceStore.Models.Entities;
using ConvenienceStore.Models.HangSo;
using ConvenienceStore.Web.DuLieuKhoiTao;
using ConvenienceStore.Web.Hubs;
using ConvenienceStore.Web.Services;
using ConvenienceStore.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BaoMatBackend.Captcha;
using BaoMatBackend.Lockout;
using BaoMatBackend.RateLimiting;
using BaoMatBackend.AutoLogout;
using BaoMatBackend.TwoFactor;
var builder = WebApplication.CreateBuilder(args);

// Ket noi database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cau hinh Identity
builder.Services.AddIdentity<NguoiDung, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;
    options.Password.RequiredUniqueChars = 4;

    options.User.RequireUniqueEmail = true;

    // Feature 3 (demo): khóa tài khoản sau 3 lần nhập sai trong 20 giây.
    LockoutConfiguration.Configure(options);
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cau hinh cookie dang nhap
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/TaiKhoan/DangNhap";
    options.AccessDeniedPath = "/TaiKhoan/TuChoiTruyCap";
    var autoLogout = builder.Configuration.GetSection(AutoLogoutOptions.SectionName).Get<AutoLogoutOptions>()
                    ?? new AutoLogoutOptions();
    AutoLogoutConfiguration.Configure(options, autoLogout.IdleSeconds, autoLogout.SlidingExpiration,
        requireHttps: !builder.Environment.IsDevelopment());
});

builder.Services.Configure<EmailOtpOptions>(builder.Configuration.GetSection(EmailOtpOptions.SectionName));
builder.Services.Configure<AutoLogoutOptions>(builder.Configuration.GetSection(AutoLogoutOptions.SectionName));
builder.Services.AddScoped<IEmailOtpService, EmailOtpService>();
builder.Services.AddScoped<IOtpEmailSender, MailKitOtpEmailSender>();
RateLimitingConfiguration.AddPolicies(builder.Services, builder.Configuration);
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
    options.ValidationInterval = TimeSpan.FromMinutes(2));

// DI
builder.Services.AddSignalR();
builder.Services.AddScoped<IDonViCongViec, DonViCongViec>();
builder.Services.AddScoped<IDichVuDanhMuc, DichVuDanhMuc>();
builder.Services.AddScoped<IDichVuSanPham, DichVuSanPham>();
builder.Services.AddScoped<IDichVuEmail, DichVuEmail>();
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<RecaptchaOptions>(builder.Configuration.GetSection(RecaptchaOptions.SectionName));
builder.Services.AddHttpClient<IRecaptchaV2Verifier, RecaptchaV2Verifier>(client => client.Timeout = TimeSpan.FromSeconds(8));
builder.Services.AddScoped<IDichVuDanhGiaSanPham, DichVuDanhGiaSanPham>();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IDichVuDashboardAdmin, DichVuDashboardAdmin>();
builder.Services.AddScoped<IDichVuDonHang, DichVuDonHang>();
builder.Services.AddScoped<IDichVuThongKeBaoCao, DichVuThongKeBaoCao>();
builder.Services.AddScoped<IDichVuDiemTichLuy, DichVuDiemTichLuy>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    var idleSeconds = builder.Configuration.GetSection(AutoLogoutOptions.SectionName)
        .Get<AutoLogoutOptions>()?.IdleSeconds ?? 20;
    options.IdleTimeout = TimeSpan.FromSeconds(Math.Max(1, idleSeconds));
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<NguoiDung>>();

    string[] dsVaiTro = { VaiTro.Admin, VaiTro.NhanVien, VaiTro.KhachHang };

    foreach (var vaiTro in dsVaiTro)
    {
        if (!await roleManager.RoleExistsAsync(vaiTro))
        {
            await roleManager.CreateAsync(new IdentityRole(vaiTro));
        }
    }

    var adminEmail = "admin@akdstore.com";
    var admin = await userManager.FindByEmailAsync(adminEmail);

    if (admin != null && !await userManager.IsInRoleAsync(admin, VaiTro.Admin))
    {
        await userManager.AddToRoleAsync(admin, VaiTro.Admin);
    }
}
using (var scope = app.Services.CreateScope())
{
    var dichVu = scope.ServiceProvider;
    await KhoiTaoVaiTroVaAdmin.KhoiTaoAsync(dichVu);
}
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.MapHub<ConvenienceStore.Web.Hubs.ChatOnlineHub>("/chatOnlineHub");
app.MapHub<GopYChatHub>("/gopyChatHub");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<IdleLogoutMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
