using ConvenienceStore.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<NguoiDung>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public DbSet<DanhGiaSanPham> DanhGiaSanPhams { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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