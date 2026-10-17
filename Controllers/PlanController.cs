using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Controllers
{
    public class PlanController : Controller
    {
        // El personal ve el ABM; el socio ve los planes solo para consultar.
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
