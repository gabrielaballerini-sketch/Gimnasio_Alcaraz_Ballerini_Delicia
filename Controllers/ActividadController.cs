using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Controllers
{

    public class ActividadController : Controller
    {
        [Authorize(Roles = "Administrador, Empleado")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}