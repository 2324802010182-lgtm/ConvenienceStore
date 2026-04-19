using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.Models.Enums;
using ConvenienceStore.Models.HangSo;
using ConvenienceStore.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ConvenienceStore.Web.Controllers
{
    [Authorize(Roles = VaiTro.Admin)]
    public class DonHangController : Controller
    {
        private readonly IDichVuDonHang _dichVuDonHang;

        public DonHangController(IDichVuDonHang dichVuDonHang)
        {
            _dichVuDonHang = dichVuDonHang;
        }

        public async Task<IActionResult> Index()
        {
            var danhSach = await _dichVuDonHang.LayTatCaAsync();

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
            var donHang = await _dichVuDonHang.LayChiTietAsync(id);
            if (donHang == null)
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

            ViewBag.DanhSachTrangThai = Enum.GetValues(typeof(TrangThaiDonHang))
                .Cast<TrangThaiDonHang>()
                .Select(x => new SelectListItem
                {
                    Value = ((int)x).ToString(),
                    Text = LayTenTrangThai(x),
                    Selected = x == donHang.TrangThai
                }).ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatTrangThai(int id, TrangThaiDonHang trangThai)
        {
            await _dichVuDonHang.CapNhatTrangThaiAsync(id, trangThai);
            TempData["ThanhCong"] = "Cập nhật trạng thái đơn hàng thành công.";
            return RedirectToAction(nameof(ChiTiet), new { id });
        }

        private string LayTenTrangThai(TrangThaiDonHang trangThai)
        {
            return trangThai switch
            {
                TrangThaiDonHang.ChoXacNhan => "Chờ xác nhận",
                TrangThaiDonHang.DaXacNhan => "Đã xác nhận",
                TrangThaiDonHang.DangChuanBi => "Đang chuẩn bị",
                TrangThaiDonHang.DangGiao => "Đang giao",
                TrangThaiDonHang.DaGiao => "Đã giao",
                TrangThaiDonHang.DaHuy => "Đã hủy",
                _ => "Không xác định"
            };
        }
    }
}