namespace ConvenienceStore.Models.Entities
{
    public class ComboSanPham
    {
        public int Id { get; set; }

        public int ComboId { get; set; }
        public Combo? Combo { get; set; }

        public int SanPhamId { get; set; }
        public SanPham? SanPham { get; set; }

        public int SoLuong { get; set; } = 1;
    }
}
