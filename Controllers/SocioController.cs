using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace Gimnasio_Alcaraz_Ballerini_Delicia.Controllers{
 
 
 [Authorize]
public class SocioController : Controller
{
    [HttpGet]
    
   
    public IActionResult Index()
    {
        return View();
    }
}}