using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.Models.HangSo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConvenienceStore.Web.Controllers
{
    [Authorize(Roles = VaiTro.Admin)]
    public class ThongKeBaoCaoController : Controller
    {
        private readonly IDichVuThongKeBaoCao _dichVuThongKeBaoCao;

        public ThongKeBaoCaoController(IDichVuThongKeBaoCao dichVuThongKeBaoCao)
        {
            _dichVuThongKeBaoCao = dichVuThongKeBaoCao;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _dichVuThongKeBaoCao.LayDuLieuBaoCaoAsync();
            return View(model);
        }
    }
}