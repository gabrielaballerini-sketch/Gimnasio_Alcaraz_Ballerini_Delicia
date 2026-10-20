using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.API
{
    [ApiController]
    [Route("api/clases")]
    public class ClaseApiController : ControllerBase
    {
        private readonly ClaseService _service;

        public ClaseApiController(ClaseService service)
        {
            _service = service;
        }


        // Listado de clases activas (filtrable por actividad y profesor)
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
            Roles = "Administrador,Empleado")]
        [HttpGet("activos")]
        public IActionResult ObtenerActivos(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamPagina = 10,
            [FromQuery] int? idActividad = null,
            [FromQuery] int? idProfesor = null)
        {
            return Ok(_service.Listar(true, idActividad, idProfesor, pagina, tamPagina));
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
            Roles = "Administrador")]
        [HttpGet("inactivos")]
        public IActionResult ObtenerInactivos(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamPagina = 10,
            [FromQuery] int? idActividad = null,
            [FromQuery] int? idProfesor = null)
        {
            return Ok(_service.Listar(false, idActividad, idProfesor, pagina, tamPagina));
        }


        // Grilla semanal completa: la puede consultar cualquier usuario logueado (incluido el socio)
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("grilla")]
        public IActionResult ObtenerGrilla()
        {
            return Ok(_service.ObtenerGrilla());
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
            Roles = "Administrador,Empleado")]
        [HttpGet("{id:int}")]
        public IActionResult Obtener(int id)
        {
            var clase = _service.Obtener(id);

            if (clase == null)
            {
                return NotFound(new { mensaje = "Clase no encontrada." });
            }

            return Ok(clase);
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
            Roles = "Administrador,Empleado")]
        [HttpPost]
        public IActionResult Crear([FromForm] Clase clase)
        {
            QuitarNavegacionDelModelo();

            if (!ModelState.IsValid)
                return ErroresDeModelo();

            try
            {
                var creada = _service.Crear(clase);

                return CreatedAtAction(
                    nameof(Obtener),
                    new { id = creada.IdClase },
                    creada);
            }
            catch (ReglaNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
            Roles = "Administrador,Empleado")]
        [HttpPut("{id:int}")]
        public IActionResult Modificar(int id, [FromForm] Clase clase)
        {
            QuitarNavegacionDelModelo();

            if (!ModelState.IsValid)
                return ErroresDeModelo();

            try
            {
                var modificada = _service.Modificar(id, clase);

                if (modificada == null)
                {
                    return NotFound(new { mensaje = "Clase no encontrada." });
                }

                return Ok(modificada);
            }
            catch (ReglaNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
            Roles = "Administrador")]
        [HttpDelete("{id:int}")]
        public IActionResult DarDeBaja(int id)
        {
            if (!_service.DarDeBaja(id))
            {
                return NotFound(new { mensaje = "Clase no encontrada." });
            }

            return NoContent();
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
            Roles = "Administrador")]
        [HttpPut("{id:int}/reactivar")]
        public IActionResult Reactivar(int id)
        {
            try
            {
                if (!_service.Reactivar(id))
                {
                    return NotFound(new { mensaje = "Clase no encontrada." });
                }

                return NoContent();
            }
            catch (ReglaNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }


        // Actividad y Profesor son propiedades de navegación: no vienen del formulario,
        // solo se usan IdActividad e IdProfesor.
        private void QuitarNavegacionDelModelo()
        {
            foreach (var key in ModelState.Keys
                .Where(k =>
                    k.StartsWith("Actividad") ||
                    k.StartsWith("Profesor"))
                .ToList())
            {
                ModelState.Remove(key);
            }
        }


        private IActionResult ErroresDeModelo()
        {
            var errores = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            return BadRequest(new
            {
                mensaje = "Hay datos inválidos.",
                errores
            });
        }
    }
}
