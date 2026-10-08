using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.API
{
    [ApiController]
    [Route("api/profesores")]

      [Authorize]
    
    public class ProfesorApiController : ControllerBase
    {
        private readonly ProfesorService _service;

        public ProfesorApiController(ProfesorService service)
        {
            _service = service;
        }

        
           [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador,Empleado")]

         
        // GET api/profesores/activos?pagina=1&tamPagina=10
        [HttpGet("activos")]
        public IActionResult ObtenerActivos(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamPagina = 10)
        {
            return Ok(
                _service.ObtenerActivos(
                    pagina,
                    tamPagina));
        }


           [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador")]

        // GET api/profesores/inactivos?pagina=1&tamPagina=10
        [HttpGet("inactivos")]
        public IActionResult ObtenerInactivos(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamPagina = 10)
        {
            return Ok(
                _service.ObtenerInactivos(
                    pagina,
                    tamPagina));
        }



           [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador,Empleado")]

        // GET api/profesores/5
        [HttpGet("{id:int}")]
        public ActionResult<Profesor> Obtener(int id)
        {
            var profesor = _service.Obtener(id);

            if (profesor == null)
                return NotFound(
                    new
                    {
                        mensaje = "Profesor no encontrado."
                    });

            return Ok(profesor);
        }

        [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador,Empleado")]  
          
        // GET api/profesores/dni/12345678
        [HttpGet("dni/{dni}")]
        public ActionResult<Profesor> ObtenerPorDni(string dni)
        {
            var profesor = _service.ObtenerPorDni(dni);

            if (profesor == null)
                return NotFound(
                    new
                    {
                        mensaje = "Profesor no encontrado."
                    });

            return Ok(profesor);
        }

        // POST api/profesores


          [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador,Empleado")]

        [HttpPost]
        public IActionResult Crear([FromForm] Profesor profesor)
        {
            if (!ModelState.IsValid)
                return ErroresDeModelo();

            try
            {
                var creado = _service.Crear(profesor);

                return CreatedAtAction(
                    nameof(Obtener),
                    new { id = creado.IdProfesor },
                    creado);
            }
            catch (ReglaNegocioException ex)
            {
                return BadRequest(
                    new
                    {
                        mensaje = ex.Message
                    });
            }
        }



            [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador,Empleado")]
        // PUT api/profesores/5
        [HttpPut("{id:int}")]
        public IActionResult Modificar(
            int id,
            [FromForm] Profesor profesor)
        {
            if (!ModelState.IsValid)
                return ErroresDeModelo();

            try
            {
                var modificado =
                    _service.Modificar(id, profesor);

                if (modificado == null)
                    return NotFound(
                        new
                        {
                            mensaje = "Profesor no encontrado."
                        });

                return Ok(modificado);
            }
            catch (ReglaNegocioException ex)
            {
                return BadRequest(
                    new
                    {
                        mensaje = ex.Message
                    });
            }
        }


             [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador")]
        // DELETE api/profesores/5
        // Baja lógica
        [HttpDelete("{id:int}")]
        public IActionResult DarDeBaja(int id)
        {
            if (!_service.DarDeBaja(id))
                return NotFound(
                    new
                    {
                        mensaje = "Profesor no encontrado."
                    });

            return NoContent();
        }

        // PUT api/profesores/5/reactivar
        [HttpPut("{id:int}/reactivar")]
        public IActionResult Reactivar(int id)
        {
            if (!_service.Reactivar(id))
                return NotFound(
                    new
                    {
                        mensaje =
                            "Profesor no encontrado o ya está activo."
                    });

            return NoContent();
        }

        private IActionResult ErroresDeModelo()
        {
            var errores = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            return BadRequest(
                new
                {
                    mensaje = "Hay datos inválidos.",
                    errores
                });
        }
    }
}