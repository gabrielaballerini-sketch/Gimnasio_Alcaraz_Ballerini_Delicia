using Microsoft.AspNetCore.Mvc;
namespace Gimnasio_Alcaraz_Ballerini_Delicia.Controllers{

public class SocioController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}}