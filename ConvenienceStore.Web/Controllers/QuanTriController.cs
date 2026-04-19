using ConvenienceStore.Business.Interfaces;
using ConvenienceStore.Models.HangSo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ConvenienceStore.Models.ViewModels;
namespace ConvenienceStore.Web.Controllers
{
    [Authorize(Roles = VaiTro.Admin)]
    public class QuanTriController : Controller
    {
        private readonly IDichVuDashboardAdmin _dichVuDashboardAdmin;

        public QuanTriController(IDichVuDashboardAdmin dichVuDashboardAdmin)
        {
            _dichVuDashboardAdmin = dichVuDashboardAdmin;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _dichVuDashboardAdmin.LayDuLieuTongQuanAsync();
            return View(model);
        }
    }
}