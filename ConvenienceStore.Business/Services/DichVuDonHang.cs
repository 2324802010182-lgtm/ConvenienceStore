using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.DataAccess;
using ConvenienceStore.DataAccess.UnitOfWork;
using ConvenienceStore.Models.Entities;
using ConvenienceStore.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.Business.Services
{
    public class DichVuDonHang : IDichVuDonHang
    {
        private readonly IDonViCongViec _donViCongViec;
        private readonly IDichVuDiemTichLuy _dichVuDiemTichLuy;
        private readonly ApplicationDbContext _context;

        public DichVuDonHang(
            IDonViCongViec donViCongViec,
            IDichVuDiemTichLuy dichVuDiemTichLuy,
            ApplicationDbContext context)
        {
            _donViCongViec = donViCongViec;
            _dichVuDiemTichLuy = dichVuDiemTichLuy;
            _context = context;
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
            PhuongThucThanhToan phuongThucThanhToan,
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
                TrangThai = TrangThaiDonHang.ChoXacNhan,
                PhuongThucThanhToan = phuongThucThanhToan,
                TrangThaiThanhToan = TrangThaiThanhToan.ChuaThanhToan
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
            PhuongThucThanhToan phuongThucThanhToan,
            int diemDaSuDung = 0,
            decimal tienGiamTuDiem = 0)
        {
            if (danhSachSanPham == null || !danhSachSanPham.Any())
                throw new Exception("Giỏ hàng đang trống.");

            decimal tongTien = 0;

            foreach (var item in danhSachSanPham)
            {
                if (item.SoLuong <= 0)
                    throw new Exception("Số lượng không hợp lệ.");

                if (item.ComboId.HasValue)
                {
                    var combo = await LayComboDayDuAsync(item.ComboId.Value);

                    if (combo == null || !combo.TrangThai)
                        throw new Exception("Combo không tồn tại hoặc đã tạm ngưng.");

                    KiemTraTonKhoCombo(combo, item.SoLuong);
                    tongTien += combo.GiaCombo * item.SoLuong;
                    continue;
                }

                var sanPham = await _donViCongViec.SanPhams.LayTheoIdAsync(item.SanPhamId);

                if (sanPham == null)
                    throw new Exception($"Không tìm thấy sản phẩm có mã {item.SanPhamId}.");

                if (sanPham.SoLuongTon < item.SoLuong)
                    throw new Exception($"Sản phẩm {sanPham.TenSanPham} không đủ số lượng tồn.");

                tongTien += sanPham.Gia * item.SoLuong;
            }

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
                TrangThai = TrangThaiDonHang.ChoXacNhan,
                PhuongThucThanhToan = phuongThucThanhToan,
                TrangThaiThanhToan = TrangThaiThanhToan.ChuaThanhToan
            };

            await _donViCongViec.DonHangs.ThemAsync(donHang);
            await _donViCongViec.LuuThayDoiAsync();

            foreach (var item in danhSachSanPham)
            {
                if (item.ComboId.HasValue)
                {
                    var combo = await LayComboDayDuAsync(item.ComboId.Value);
                    if (combo == null) continue;

                    var sanPhamDaiDien = combo.ComboSanPhams.First().SanPham;
                    if (sanPhamDaiDien == null) continue;

                    var chiTietCombo = new ChiTietDonHang
                    {
                        DonHangId = donHang.Id,
                        SanPhamId = sanPhamDaiDien.Id,
                        ComboId = combo.Id,
                        LaCombo = true,
                        TenHienThi = combo.TenCombo,
                        HinhAnhHienThi = combo.HinhAnh ?? sanPhamDaiDien.HinhAnh,
                        MoTaCombo = string.Join(" + ", combo.ComboSanPhams.Select(x => x.SanPham?.TenSanPham).Where(x => !string.IsNullOrWhiteSpace(x))),
                        SoLuong = item.SoLuong,
                        DonGia = combo.GiaCombo
                    };

                    await _donViCongViec.ChiTietDonHangs.ThemAsync(chiTietCombo);

                    foreach (var thanhPhan in combo.ComboSanPhams)
                    {
                        if (thanhPhan.SanPham == null) continue;

                        thanhPhan.SanPham.SoLuongTon -= item.SoLuong * thanhPhan.SoLuong;
                        _donViCongViec.SanPhams.Sua(thanhPhan.SanPham);
                    }

                    continue;
                }

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

        private async Task<Combo?> LayComboDayDuAsync(int comboId)
        {
            return await _context.Combos
                .Include(x => x.ComboSanPhams)
                    .ThenInclude(x => x.SanPham)
                .FirstOrDefaultAsync(x => x.Id == comboId && x.ComboSanPhams.Count >= 2);
        }

        private static void KiemTraTonKhoCombo(Combo combo, int soLuongCombo)
        {
            foreach (var thanhPhan in combo.ComboSanPhams)
            {
                if (thanhPhan.SanPham == null)
                    throw new Exception("Combo có sản phẩm không hợp lệ.");

                var soLuongCan = soLuongCombo * thanhPhan.SoLuong;

                if (thanhPhan.SanPham.SoLuongTon < soLuongCan)
                {
                    throw new Exception($"Sản phẩm {thanhPhan.SanPham.TenSanPham} không đủ số lượng tồn cho combo.");
                }
            }
        }
    }
}
