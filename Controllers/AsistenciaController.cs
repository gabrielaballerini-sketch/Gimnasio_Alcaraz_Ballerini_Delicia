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

        // Historial completo: el personal ve todo; el socio solo lo suyo (lo resuelve la API).
        [Authorize(Roles = "Administrador,Empleado,Socio")]
        [HttpGet]
        public IActionResult Historial()
        {
            return View();
        }

        // Utilización del plan en el período actual (solo socio)
        [Authorize(Roles = "Socio")]
        [HttpGet]
        public IActionResult MiUtilizacion()
        {
            return View();
        }
    }
}