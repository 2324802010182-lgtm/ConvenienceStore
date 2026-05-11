using ConvenienceStore.Models.Entities;

namespace ConvenienceStore.Web.ViewModels
{
    public class ChiTietChatViewModel
    {
        public HoiThoaiChat HoiThoai { get; set; } = new HoiThoaiChat();
        public List<TinNhanChat> DanhSachTinNhan { get; set; } = new List<TinNhanChat>();
    }
}