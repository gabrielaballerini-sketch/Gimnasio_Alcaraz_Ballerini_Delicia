using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;



namespace Gimnasio_Alcaraz_Ballerini_Delicia.Controllers
{


     [Authorize(Roles ="Administrador")]
    public class UsuarioController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}