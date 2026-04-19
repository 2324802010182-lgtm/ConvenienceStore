using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConvenienceStore.Web.Controllers
{
    [Authorize]
    public class DonHangCuaToiController : Controller
    {
        private readonly IDichVuDonHang _dichVuDonHang;

        public DonHangCuaToiController(IDichVuDonHang dichVuDonHang)
        {
            _dichVuDonHang = dichVuDonHang;
        }

        public async Task<IActionResult> Index()
        {
            var nguoiDungId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(nguoiDungId))
                return Challenge();

            var danhSach = await _dichVuDonHang.LayDonHangTheoNguoiDungAsync(nguoiDungId);

            var model = danhSach.Select(x => new DonHangAdminViewModel
            {
                Id = x.Id,
                HoTenNguoiNhan = x.HoTenNguoiNhan,
                EmailNguoiDat = x.NguoiDung != null ? x.NguoiDung.Email : "",
                SoDienThoai = x.SoDienThoai,
                TongTien = x.TongTien,
                NgayDatHang = x.NgayDatHang,
                TrangThai = LayTenTrangThai(x.TrangThai)
            }).ToList();

            return View(model);
        }

        public async Task<IActionResult> ChiTiet(int id)
        {
            var nguoiDungId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(nguoiDungId))
                return Challenge();

            var donHang = await _dichVuDonHang.LayChiTietAsync(id);
            if (donHang == null || donHang.NguoiDungId != nguoiDungId)
                return NotFound();

            var model = new ChiTietDonHangAdminViewModel
            {
                Id = donHang.Id,
                HoTenNguoiNhan = donHang.HoTenNguoiNhan,
                SoDienThoai = donHang.SoDienThoai,
                DiaChiNhanHang = donHang.DiaChiNhanHang,
                EmailNguoiDat = donHang.NguoiDung?.Email,
                TongTien = donHang.TongTien,
                NgayDatHang = donHang.NgayDatHang,
                TrangThai = LayTenTrangThai(donHang.TrangThai),
                DanhSachSanPham = donHang.ChiTietDonHangs?.Select(ct => new ChiTietSanPhamTrongDonViewModel
                {
                    TenSanPham = ct.SanPham != null ? ct.SanPham.TenSanPham : "",
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGia,
                    ThanhTien = ct.ThanhTien
                }).ToList() ?? new List<ChiTietSanPhamTrongDonViewModel>()
            };

            return View(model);
        }

        private string LayTenTrangThai(ConvenienceStore.Models.Enums.TrangThaiDonHang trangThai)
        {
            return trangThai switch
            {
                ConvenienceStore.Models.Enums.TrangThaiDonHang.ChoXacNhan => "Chờ xác nhận",
                ConvenienceStore.Models.Enums.TrangThaiDonHang.DaXacNhan => "Đã xác nhận",
                ConvenienceStore.Models.Enums.TrangThaiDonHang.DangChuanBi => "Đang chuẩn bị",
                ConvenienceStore.Models.Enums.TrangThaiDonHang.DangGiao => "Đang giao",
                ConvenienceStore.Models.Enums.TrangThaiDonHang.DaGiao => "Đã giao",
                ConvenienceStore.Models.Enums.TrangThaiDonHang.DaHuy => "Đã hủy",
                _ => "Không xác định"
            };
        }
    }
}