using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Controllers
{
    [Authorize(Roles = "Socio,Administrador,Empleado")]
    public class PagoController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            if (User.IsInRole("Socio"))
                return View("IndexSocio");

            return View();
        }
    }
}