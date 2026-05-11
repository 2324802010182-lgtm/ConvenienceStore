using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ConvenienceStore.Web.Hubs
{
    [Authorize]
    public class GopYChatHub : Hub
    {
        public async Task ThamGiaPhong(string maPhong)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, maPhong);
        }

        public async Task RoiPhong(string maPhong)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, maPhong);
        }
    }
}