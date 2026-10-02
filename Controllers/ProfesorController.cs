using Microsoft.AspNetCore.Mvc;
namespace Gimnasio_Alcaraz_Ballerini_Delicia.Controllers
{

    public class ProfesorController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}