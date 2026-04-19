using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ConvenienceStore.Web.Controllers
{
    public class SanPhamKhachController : Controller
    {
        private readonly IDichVuSanPham _dichVuSanPham;

        public SanPhamKhachController(IDichVuSanPham dichVuSanPham)
        {
            _dichVuSanPham = dichVuSanPham;
        }

        public async Task<IActionResult> Index()
        {
            var danhSachSanPham = await _dichVuSanPham.LaySanPhamDangBanAsync();
            return View(danhSachSanPham);
        }

        public async Task<IActionResult> TheoDanhMuc(int id)
        {
            var danhSachSanPham = await _dichVuSanPham.LaySanPhamTheoDanhMucAsync(id);
            return View("Index", danhSachSanPham);
        }

        public async Task<IActionResult> ChiTiet(int id)
        {
            var sanPham = await _dichVuSanPham.LayTheoIdKemDanhMucAsync(id);
            if (sanPham == null)
                return NotFound();

            var tatCaSanPham = await _dichVuSanPham.LaySanPhamDangBanAsync();

            var sanPhamLienQuan = tatCaSanPham
                .Where(x => x.Id != sanPham.Id && x.DanhMucId == sanPham.DanhMucId)
                .Take(4)
                .ToList();

            var model = new ChiTietSanPhamKhachViewModel
            {
                SanPham = sanPham,
                SanPhamLienQuan = sanPhamLienQuan
            };

            return View(model);
        }

        public async Task<IActionResult> TimKiem(string tuKhoa)
        {
            var ketQua = await _dichVuSanPham.TimKiemSanPhamAsync(tuKhoa);
            ViewBag.TuKhoa = tuKhoa;
            return View(ketQua);
        }
    }
}