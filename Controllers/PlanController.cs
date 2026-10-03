using Microsoft.AspNetCore.Mvc;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Controllers
{
    public class PlanController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}