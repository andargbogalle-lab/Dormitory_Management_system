using dprmitory.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace dprmitory.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Show home page regardless of login status
            // Users can navigate to login via the navbar icon
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
