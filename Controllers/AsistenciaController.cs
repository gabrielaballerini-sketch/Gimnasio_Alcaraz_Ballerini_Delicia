
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Controllers
{
    public class AsistenciaController : Controller
    {
        // Pantalla de registro por DNI.
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // Historial reservado al personal del gimnasio.
        [Authorize(Roles = "Administrador,Empleado")]
        [HttpGet]
        public IActionResult Historial()
        {
            return View();
        }
    }
}
