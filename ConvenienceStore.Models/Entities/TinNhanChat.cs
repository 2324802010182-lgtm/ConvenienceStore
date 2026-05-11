using System.ComponentModel.DataAnnotations;

namespace ConvenienceStore.Models.Entities
{
    public class TinNhanChat
    {
        public int Id { get; set; }

        [Required]
        public int HoiThoaiChatId { get; set; }
        public HoiThoaiChat? HoiThoaiChat { get; set; }

        [Required]
        public string NguoiGuiId { get; set; } = string.Empty;
        public NguoiDung? NguoiGui { get; set; }

        [StringLength(2000)]
        public string? NoiDung { get; set; }

        public string? HinhAnh { get; set; }

        public DateTime NgayGui { get; set; } = DateTime.Now;
    }
}