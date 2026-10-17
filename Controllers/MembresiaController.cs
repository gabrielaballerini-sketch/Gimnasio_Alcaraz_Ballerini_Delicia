using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Controllers
{
    public class MembresiaController : Controller
    {
        // El personal ve el ABM; el socio ve solo su propia membresía.
        [Authorize(Roles = "Socio, Administrador, Empleado")]
        [HttpGet]
        public IActionResult Index()
        {
            if (User.IsInRole("Socio"))
                return View("IndexSocio");

            return View();
        }
    }
}
