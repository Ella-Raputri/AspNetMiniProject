using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RentalCarFront.Models;
using RentalCarFront.Models.Output;
using RentalCarFront.Service;

namespace RentalCarFront.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ICar _carApi;

    public HomeController(ILogger<HomeController> logger, ICar carApi)
    {
        _logger = logger;
        _carApi = carApi;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Kontak()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public async Task<IActionResult> GetAvailableCars(DateTime dateA, DateTime dateB, int? year = null){
        var result = await _carApi.GetAvailableCars(dateA, dateB, year);
        return Json(result);
    }
    public async Task<IActionResult> GetCarInformation(DateTime dateStart, DateTime dateEnd){
        var result = await _carApi.GetCarInformation(dateStart, dateEnd);
        return Json(result);
    }
}
