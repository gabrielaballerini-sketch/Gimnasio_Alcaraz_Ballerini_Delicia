using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Controllers
{
    public class PlanController : Controller
    {
        [Authorize(Roles = "Administrador, Empleado")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}