using Microsoft.AspNetCore.Mvc;
using SistemaWebElimCentral.Models;
using System.Diagnostics;

namespace SistemaWebElimCentral.Controllers
{
    public class HomeController : Controller
    {
        // La página de bienvenida de la plantilla ya no se usa:
        // se redirige al login (que a su vez manda al Dashboard si ya hay sesión)
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Login");
        }

        public IActionResult Privacy()
        {
            return RedirectToAction("Index", "Login");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}