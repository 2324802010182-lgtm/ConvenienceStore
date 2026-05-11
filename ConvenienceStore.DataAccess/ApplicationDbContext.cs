using ConvenienceStore.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ConvenienceStore.Models.Entities;
using ConvenienceStore.Models.Enums;
namespace ConvenienceStore.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<NguoiDung>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<GopY> GopYs { get; set; }
        public DbSet<HoiThoaiChat> HoiThoaiChats { get; set; }
        public DbSet<TinNhanChat> TinNhanChats { get; set; }
        public DbSet<TinNhanGopY> TinNhanGopYs { get; set; }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public DbSet<DanhGiaSanPham> DanhGiaSanPhams { get; set; }
        public DbSet<LichSuDiem> LichSuDiems { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HoiThoaiChat>().ToTable("HoiThoaiChats");
            modelBuilder.Entity<TinNhanChat>().ToTable("TinNhanChats");

            modelBuilder.Entity<HoiThoaiChat>()
                .HasOne(x => x.KhachHang)
                .WithMany()
                .HasForeignKey(x => x.KhachHangId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HoiThoaiChat>()
                .HasOne(x => x.NhanVienPhuTrach)
                .WithMany()
                .HasForeignKey(x => x.NhanVienPhuTrachId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TinNhanChat>()
                .HasOne(x => x.HoiThoaiChat)
                .WithMany(x => x.TinNhans)
                .HasForeignKey(x => x.HoiThoaiChatId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TinNhanChat>()
                .HasOne(x => x.NguoiGui)
                .WithMany()
                .HasForeignKey(x => x.NguoiGuiId)
                .OnDelete(DeleteBehavior.Restrict);
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<GopY>().ToTable("GopYs");
            modelBuilder.Entity<TinNhanGopY>().ToTable("TinNhanGopYs");

            modelBuilder.Entity<GopY>()
                .HasOne(x => x.NguoiGui)
                .WithMany()
                .HasForeignKey(x => x.NguoiGuiId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GopY>()
                .HasOne(x => x.SanPham)
                .WithMany()
                .HasForeignKey(x => x.SanPhamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GopY>()
                .HasOne(x => x.NhanVien)
                .WithMany()
                .HasForeignKey(x => x.NhanVienId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TinNhanGopY>()
                .HasOne(x => x.GopY)
                .WithMany(x => x.TinNhans)
                .HasForeignKey(x => x.GopYId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TinNhanGopY>()
                .HasOne(x => x.NguoiGui)
                .WithMany()
                .HasForeignKey(x => x.NguoiGuiId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DanhMuc>().ToTable("DanhMucs");
            modelBuilder.Entity<SanPham>().ToTable("SanPhams");
            modelBuilder.Entity<DonHang>().ToTable("DonHangs");
            modelBuilder.Entity<ChiTietDonHang>().ToTable("ChiTietDonHangs");
            modelBuilder.Entity<DanhGiaSanPham>().ToTable("DanhGiaSanPhams");
            modelBuilder.Entity<DanhGiaSanPham>()
                 .HasOne(x => x.SanPham)
                 .WithMany()
                 .HasForeignKey(x => x.SanPhamId)
                 .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<LichSuDiem>()
                .Property(x => x.SoTienTuongUng)
                .HasPrecision(18, 2);
            modelBuilder.Entity<DanhGiaSanPham>()
                .HasOne(x => x.NguoiDung)
                .WithMany()
                .HasForeignKey(x => x.NguoiDungId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DanhMuc>()
                .HasMany(dm => dm.DanhSachSanPham)
                .WithOne(sp => sp.DanhMuc)
                .HasForeignKey(sp => sp.DanhMucId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DonHang>()
                .Property(x => x.TienGiamTuDiem)
                .HasPrecision(18, 2);
            modelBuilder.Entity<DonHang>()
                .Property(x => x.TongTien)
                .HasPrecision(18, 2);
            modelBuilder.Entity<DonHang>()
                .Property(x => x.TongTienSauGiam)
                .HasPrecision(18, 2);
            modelBuilder.Entity<DonHang>()
                .HasMany(dh => dh.ChiTietDonHangs)
                .WithOne(ct => ct.DonHang)
                .HasForeignKey(ct => ct.DonHangId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.SanPham)
                .WithMany()
                .HasForeignKey(ct => ct.SanPhamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DonHang>()
                .HasOne(dh => dh.NguoiDung)
                .WithMany()
                .HasForeignKey(dh => dh.NguoiDungId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}