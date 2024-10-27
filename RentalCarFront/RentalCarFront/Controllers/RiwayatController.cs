using Microsoft.AspNetCore.Mvc;
using RentalCarFront.Service;

namespace RentalCarFront.Controllers
{
    public class RiwayatController : Controller
    {
        private readonly IRiwayat _riwayatApi;

        public RiwayatController(IRiwayat riwayatApi)
        {
            _riwayatApi = riwayatApi;
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetHistoryUser(string id)
        {
            var result = await _riwayatApi.GetHistoryUser(id);
            return Json(result);
        }

    }
}
