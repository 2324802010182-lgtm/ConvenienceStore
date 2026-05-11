using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ConvenienceStore.Web.Hubs
{
    [Authorize]
    public class ChatOnlineHub : Hub
    {
        public async Task ThamGiaHoiThoai(string maPhong)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, maPhong);
        }

        public async Task RoiHoiThoai(string maPhong)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, maPhong);
        }

        public async Task ThamGiaNhomHoTro()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "NHOM_HO_TRO");
        }
    }
}