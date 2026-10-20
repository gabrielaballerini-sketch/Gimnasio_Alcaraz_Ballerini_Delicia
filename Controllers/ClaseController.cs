using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Controllers
{
    public class ClaseController : Controller
    {
        // El personal administra las clases; el socio va directo a la grilla.
        [Authorize(Roles = "Socio, Administrador, Empleado")]
        [HttpGet]
        public IActionResult Index()
        {
            if (User.IsInRole("Socio"))
                return RedirectToAction(nameof(Grilla));

            return View();
        }

        // Grilla semanal de solo lectura: la ven todos los roles
        [Authorize(Roles = "Socio, Administrador, Empleado")]
        [HttpGet]
        public IActionResult Grilla()
        {
            return View();
        }
    }
}
