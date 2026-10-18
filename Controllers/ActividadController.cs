using Gimnasio_Alcaraz_Ballerini_Delicia.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Controllers
{

    public class ActividadController : Controller
    {
        private readonly ActividadService _service;

        public ActividadController(ActividadService service)
        {
            _service = service;
        }


        [AllowAnonymous]
        [HttpGet]
        public IActionResult Index()
        {
            if (User.IsInRole("Socio") || User.Identity?.IsAuthenticated != true)
                return View("IndexSocio");

            return View();
        }


        // Detalle con carrusel de fotos y videos
        [Authorize(Roles = "Socio, Administrador, Empleado")]
        [HttpGet]
        public IActionResult Detalle(int id)
        {
            var actividad = _service.Obtener(id);

            if (actividad == null)
                return NotFound();

            return View(actividad);
        }
    }
}
