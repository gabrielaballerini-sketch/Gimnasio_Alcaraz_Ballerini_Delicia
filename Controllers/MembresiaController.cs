using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Controllers
{
    public class MembresiaController : Controller
    {
        [Authorize(Roles = "Administrador, Empleado")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}