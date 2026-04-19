namespace ConvenienceStore.Web.ViewModels
{
    public class NguoiDungAdminViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string? HoTen { get; set; }
        public string? Email { get; set; }
        public string VaiTro { get; set; } = "Khách hàng";
        public bool BiKhoa { get; set; }
        public DateTimeOffset? KhoaDenNgay { get; set; }
    }
}