using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.Business.Services;
using ConvenienceStore.DataAccess;
using ConvenienceStore.DataAccess.UnitOfWork;
using ConvenienceStore.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ConvenienceStore.Web.Services;
using ConvenienceStore.Web.ViewModels;
using ConvenienceStore.Web.DuLieuKhoiTao;
using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.Business.Services;
using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.Business.Services;
var builder = WebApplication.CreateBuilder(args);

// Ket noi database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cau hinh Identity
builder.Services.AddIdentity<NguoiDung, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cau hinh cookie dang nhap
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/TaiKhoan/DangNhap";
    options.AccessDeniedPath = "/TaiKhoan/TuChoiTruyCap";
});

// DI
builder.Services.AddScoped<IDonViCongViec, DonViCongViec>();
builder.Services.AddScoped<IDichVuDanhMuc, DichVuDanhMuc>();
builder.Services.AddScoped<IDichVuSanPham, DichVuSanPham>();
builder.Services.AddScoped<IDichVuEmail, DichVuEmail>();
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IDichVuEmail, DichVuEmail>();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IDichVuDashboardAdmin, DichVuDashboardAdmin>();
builder.Services.AddScoped<IDichVuDonHang, DichVuDonHang>();
builder.Services.AddScoped<IDichVuThongKeBaoCao, DichVuThongKeBaoCao>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
var app = builder.Build();
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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();