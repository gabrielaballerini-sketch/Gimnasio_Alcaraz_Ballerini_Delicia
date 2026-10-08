using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Gimnasio_Alcaraz_Ballerini_Delicia.Service;
namespace Gimnasio_Alcaraz_Ballerini_Delicia.API{

[ApiController]
[Route("api/socios")]

  [Authorize]
public class SocioApiController : ControllerBase
{
    private readonly SocioService _service;

    public SocioApiController(SocioService service)
    {
        _service = service;
    }


    
     [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador,Empleado")]

    // GET api/socios/activos?pagina=1&tamPagina=10
    [HttpGet("activos")]
    public IActionResult ObtenerActivos([FromQuery] int pagina = 1, [FromQuery] int tamPagina = 10)
    {
        return Ok(_service.ObtenerActivos(pagina, tamPagina));
    }



     
           [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador")]
    // GET api/socios/inactivos?pagina=1&tamPagina=10
    [HttpGet("inactivos")]
    public IActionResult ObtenerInactivos([FromQuery] int pagina = 1, [FromQuery] int tamPagina = 10)
    {
        return Ok(_service.ObtenerInactivos(pagina, tamPagina));
    }

    // GET api/socios/5
    [HttpGet("{id:int}")]
    public ActionResult<Socio> Obtener(int id)
    {
        var socio = _service.Obtener(id);
        if (socio == null) return NotFound(new { mensaje = "Socio no encontrado." });
        return Ok(socio);
    }



     
           [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador,Empleado")] 
    
    // POST api/socios  (multipart/form-data)
    [HttpPost]
    public IActionResult Crear([FromForm] Socio socio)
    {
        if (!ModelState.IsValid) return ErroresDeModelo();

        try
        {
            var creado = _service.Crear(socio);
            return CreatedAtAction(nameof(Obtener), new { id = creado.IdSocio }, creado);
        }
        catch (ReglaNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }


     
           [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador,Empleado")]
    // PUT api/socios/5  (multipart/form-data)
    [HttpPut("{id:int}")]
    public IActionResult Modificar(int id, [FromForm] Socio socio)
    {
        if (!ModelState.IsValid) return ErroresDeModelo();

        try
        {
            var modificado = _service.Modificar(id, socio);
            if (modificado == null) return NotFound(new { mensaje = "Socio no encontrado." });
            return Ok(modificado);
        }
        catch (ReglaNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }


     
           [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador")]
    // DELETE api/socios/5  (baja lógica)
    [HttpDelete("{id:int}")]
    public IActionResult DarDeBaja(int id)
    {
        if (!_service.DarDeBaja(id))
            return NotFound(new { mensaje = "Socio no encontrado." });
        return NoContent();
    }

    // PUT api/socios/5/reactivar
    [HttpPut("{id:int}/reactivar")]
    public IActionResult Reactivar(int id)
    {
        if (!_service.Reactivar(id))
            return NotFound(new { mensaje = "Socio no encontrado o ya está activo." });
        return NoContent();
    }

    private IActionResult ErroresDeModelo()
    {
        var errores = ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .Distinct()
            .ToList();

        return BadRequest(new { mensaje = "Hay datos inválidos.", errores });
    }
}}