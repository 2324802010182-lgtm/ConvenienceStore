using ConvenienceStore.Models.ViewModels;

namespace ConvenienceStore.Business.Interfaces
{
    public interface IDichVuDashboardAdmin
    {
        Task<DashboardAdminViewModel> LayDuLieuTongQuanAsync();
    }
}