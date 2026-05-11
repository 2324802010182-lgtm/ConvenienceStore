using System.ComponentModel.DataAnnotations;

namespace ConvenienceStore.Models.Entities
{
    public class TinNhanGopY
    {
        public int Id { get; set; }

        [Required]
        public int GopYId { get; set; }
        public GopY? GopY { get; set; }

        [Required]
        public string NguoiGuiId { get; set; } = string.Empty;
        public NguoiDung? NguoiGui { get; set; }

        [StringLength(2000)]
        public string? NoiDung { get; set; }

        public string? HinhAnh { get; set; }

        public DateTime NgayGui { get; set; } = DateTime.Now;
    }
}