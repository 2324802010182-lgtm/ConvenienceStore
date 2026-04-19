using ConvenienceStore.Models.ViewModels;

namespace ConvenienceStore.Business.Interfaces
{
    public interface IDichVuThongKeBaoCao
    {
        Task<ThongKeBaoCaoViewModel> LayDuLieuBaoCaoAsync();
    }
}