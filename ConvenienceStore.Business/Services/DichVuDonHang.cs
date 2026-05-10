using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.DataAccess.UnitOfWork;
using ConvenienceStore.Models.Entities;
using ConvenienceStore.Models.Enums;

namespace ConvenienceStore.Business.Services
{
    public class DichVuDonHang : IDichVuDonHang
    {
        private readonly IDonViCongViec _donViCongViec;
        private readonly IDichVuDiemTichLuy _dichVuDiemTichLuy;

        public DichVuDonHang(
            IDonViCongViec donViCongViec,
            IDichVuDiemTichLuy dichVuDiemTichLuy)
        {
            _donViCongViec = donViCongViec;
            _dichVuDiemTichLuy = dichVuDiemTichLuy;
        }

        public async Task<IEnumerable<DonHang>> LayTatCaAsync()
        {
            return await _donViCongViec.DonHangs.LayTatCaKemNguoiDungAsync();
        }

        public async Task<DonHang?> LayChiTietAsync(int id)
        {
            return await _donViCongViec.DonHangs.LayChiTietDonHangAsync(id);
        }

        public async Task<IEnumerable<DonHang>> LayDonHangTheoNguoiDungAsync(string nguoiDungId)
        {
            return await _donViCongViec.DonHangs.LayTheoNguoiDungAsync(nguoiDungId);
        }

        public async Task CapNhatTrangThaiAsync(int id, TrangThaiDonHang trangThai)
        {
            var donHang = await _donViCongViec.DonHangs.LayTheoIdAsync(id);

            if (donHang == null)
                return;

            var trangThaiCu = donHang.TrangThai;

            donHang.TrangThai = trangThai;

            _donViCongViec.DonHangs.Sua(donHang);
            await _donViCongViec.LuuThayDoiAsync();

            if (trangThaiCu != TrangThaiDonHang.DaGiao &&
                trangThai == TrangThaiDonHang.DaGiao)
            {
                var soTienTinhDiem = donHang.TongTienSauGiam > 0
                    ? donHang.TongTienSauGiam
                    : donHang.TongTien;

                await _dichVuDiemTichLuy.CongDiemSauThanhToanAsync(
                    donHang.NguoiDungId,
                    donHang.Id,
                    soTienTinhDiem);
            }
        }

        public async Task<int> TaoDonHangAsync(
            string nguoiDungId,
            int sanPhamId,
            int soLuong,
            string hoTenNguoiNhan,
            string soDienThoai,
            string diaChiNhanHang,
            int diemDaSuDung = 0,
            decimal tienGiamTuDiem = 0)
        {
            var sanPham = await _donViCongViec.SanPhams.LayTheoIdAsync(sanPhamId);

            if (sanPham == null)
                throw new Exception("Không tìm thấy sản phẩm.");

            if (soLuong <= 0)
                throw new Exception("Số lượng không hợp lệ.");

            if (sanPham.SoLuongTon < soLuong)
                throw new Exception("Sản phẩm không đủ số lượng tồn.");

            var tongTien = sanPham.Gia * soLuong;
            if (diemDaSuDung < 0)
                diemDaSuDung = 0;

            if (tienGiamTuDiem < 0)
                tienGiamTuDiem = 0;

            if (tienGiamTuDiem > tongTien)
                tienGiamTuDiem = tongTien;

            var tongTienSauGiam = tongTien - tienGiamTuDiem;
            var donHang = new DonHang
            {
                NguoiDungId = nguoiDungId,
                HoTenNguoiNhan = hoTenNguoiNhan,
                SoDienThoai = soDienThoai,
                DiaChiNhanHang = diaChiNhanHang,
                TongTien = tongTien,
                DiemDaSuDung = diemDaSuDung,
                TienGiamTuDiem = tienGiamTuDiem,
                TongTienSauGiam = tongTienSauGiam,
                NgayDatHang = DateTime.Now,
                TrangThai = TrangThaiDonHang.ChoXacNhan
            };

            await _donViCongViec.DonHangs.ThemAsync(donHang);
            await _donViCongViec.LuuThayDoiAsync();

            var chiTietDonHang = new ChiTietDonHang
            {
                DonHangId = donHang.Id,
                SanPhamId = sanPham.Id,
                SoLuong = soLuong,
                DonGia = sanPham.Gia
            };

            await _donViCongViec.ChiTietDonHangs.ThemAsync(chiTietDonHang);

            sanPham.SoLuongTon -= soLuong;
            _donViCongViec.SanPhams.Sua(sanPham);

            await _donViCongViec.LuuThayDoiAsync();

            return donHang.Id;
        }

        public async Task<int> TaoDonHangTuGioAsync(
            string nguoiDungId,
            List<SanPhamDatHang> danhSachSanPham,
            string hoTenNguoiNhan,
            string soDienThoai,
            string diaChiNhanHang,
            int diemDaSuDung = 0,
            decimal tienGiamTuDiem = 0)
        {
            if (danhSachSanPham == null || !danhSachSanPham.Any())
                throw new Exception("Giỏ hàng đang trống.");

            decimal tongTien = 0;

            foreach (var item in danhSachSanPham)
            {
                var sanPham = await _donViCongViec.SanPhams.LayTheoIdAsync(item.SanPhamId);

                if (sanPham == null)
                    throw new Exception($"Không tìm thấy sản phẩm có mã {item.SanPhamId}.");

                if (item.SoLuong <= 0)
                    throw new Exception("Số lượng không hợp lệ.");

                if (sanPham.SoLuongTon < item.SoLuong)
                    throw new Exception($"Sản phẩm {sanPham.TenSanPham} không đủ số lượng tồn.");

                tongTien += sanPham.Gia * item.SoLuong;
            }

            // Đoạn này phải nằm ngoài foreach
            if (diemDaSuDung < 0)
                diemDaSuDung = 0;

            if (tienGiamTuDiem < 0)
                tienGiamTuDiem = 0;

            if (tienGiamTuDiem > tongTien)
                tienGiamTuDiem = tongTien;

            var tongTienSauGiam = tongTien - tienGiamTuDiem;

            var donHang = new DonHang
            {
                NguoiDungId = nguoiDungId,
                HoTenNguoiNhan = hoTenNguoiNhan,
                SoDienThoai = soDienThoai,
                DiaChiNhanHang = diaChiNhanHang,

                TongTien = tongTien,
                DiemDaSuDung = diemDaSuDung,
                TienGiamTuDiem = tienGiamTuDiem,
                TongTienSauGiam = tongTienSauGiam,

                NgayDatHang = DateTime.Now,
                TrangThai = TrangThaiDonHang.ChoXacNhan
            };

            await _donViCongViec.DonHangs.ThemAsync(donHang);
            await _donViCongViec.LuuThayDoiAsync();

            foreach (var item in danhSachSanPham)
            {
                var sanPham = await _donViCongViec.SanPhams.LayTheoIdAsync(item.SanPhamId);
                if (sanPham == null) continue;

                var chiTiet = new ChiTietDonHang
                {
                    DonHangId = donHang.Id,
                    SanPhamId = sanPham.Id,
                    SoLuong = item.SoLuong,
                    DonGia = sanPham.Gia
                };

                await _donViCongViec.ChiTietDonHangs.ThemAsync(chiTiet);

                sanPham.SoLuongTon -= item.SoLuong;
                _donViCongViec.SanPhams.Sua(sanPham);
            }

            await _donViCongViec.LuuThayDoiAsync();

            return donHang.Id;
        }
    }
}