using Microsoft.AspNetCore.Mvc;

namespace MovieApp.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult NotFound()
    {
        return View("~/Views/Shared/NotFound.cshtml");
    }
}
