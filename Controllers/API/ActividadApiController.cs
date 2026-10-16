using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.API
{
    [ApiController]
    [Route("api/actividades")]



    public class ActividadApiController : ControllerBase
    {
        private readonly ActividadService _service;


        public ActividadApiController(
            ActividadService service)
        {
            _service = service;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("activos")]
        public IActionResult ObtenerActivos(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamPagina = 10)
        {
            return Ok(
                _service.ObtenerActivos(
                    pagina,
                    tamPagina
                )
            );
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
       Roles = "Administrador")]


        [HttpGet("inactivos")]
        public IActionResult ObtenerInactivos(
           [FromQuery] int pagina = 1,
           [FromQuery] int tamPagina = 10)
        {
            return Ok(
                _service.ObtenerInactivos(
                    pagina,
                    tamPagina
                )
            );
        }



        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Administrador,Empleado")]


        [HttpGet("{id:int}")]
        public ActionResult<Actividad> Obtener(
        int id)
        {
            var actividad =
                _service.Obtener(id);

            if (actividad == null)
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            "Actividad no encontrada."
                    }
                );
            }

            return Ok(actividad);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Administrador,Empleado")]


        [HttpPost]
        public IActionResult Crear(
        [FromForm] Actividad actividad)
        {
            if (!ModelState.IsValid)
                return ErroresDeModelo();

            try
            {
                var creado =
                    _service.Crear(actividad);

                return CreatedAtAction(
                    nameof(Obtener),
                    new
                    {
                        id = creado.IdActividad
                    },
                    creado
                );
            }
            catch (ReglaNegocioException ex)
            {
                return BadRequest(
                    new
                    {
                        mensaje = ex.Message
                    }
                );
            }
        }



        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Administrador,Empleado")]



        [HttpPut("{id:int}")]
        public IActionResult Modificar(
        int id,
        [FromForm] Actividad actividad)
        {
            if (!ModelState.IsValid)
                return ErroresDeModelo();

            try
            {
                var modificado =
                    _service.Modificar(
                        id,
                        actividad
                    );

                if (modificado == null)
                {
                    return NotFound(
                        new
                        {
                            mensaje =
                                "Actividad no encontrada."
                        }
                    );
                }

                return Ok(modificado);
            }
            catch (ReglaNegocioException ex)
            {
                return BadRequest(
                    new
                    {
                        mensaje = ex.Message
                    }
                );
            }
        }



        // SUBIR O REEMPLAZAR IMAGEN / VIDEO
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Administrador,Empleado")]
        [HttpPut("{id:int}/archivo")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(25 * 1024 * 1024)]
        public async Task<IActionResult> SubirArchivo(
            int id,
            [FromForm] IFormFile? archivo)
        {
            if (archivo == null)
            {
                return BadRequest(new
                {
                    mensaje = "Debe seleccionar una imagen o un video."
                });
            }

            try
            {
                var actividad = await _service.SubirArchivo(id, archivo);

                if (actividad == null)
                {
                    return NotFound(new
                    {
                        mensaje = "Actividad no encontrada."
                    });
                }

                return Ok(actividad);
            }
            catch (ReglaNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }



        // QUITAR IMAGEN / VIDEO
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Administrador,Empleado")]
        [HttpDelete("{id:int}/archivo")]
        public IActionResult QuitarArchivo(int id)
        {
            var actividad = _service.QuitarArchivo(id);

            if (actividad == null)
            {
                return NotFound(new
                {
                    mensaje = "Actividad no encontrada."
                });
            }

            return Ok(actividad);
        }



        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Administrador")]
        [HttpDelete("{id:int}")]
        public IActionResult DarDeBaja(
        int id)
        {
            if (!_service.DarDeBaja(id))
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            "Actividad no encontrada."
                    }
                );
            }

            return NoContent();
        }




        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Administrador")]
        [HttpPut("{id:int}/reactivar")]
        public IActionResult Reactivar(
        int id)
        {
            if (!_service.Reactivar(id))
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            "Actividad no encontrada o ya está activa."
                    }
                );
            }

            return NoContent();
        }


        private IActionResult ErroresDeModelo()
        {
            var errores =
                ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Distinct()
                    .ToList();

            return BadRequest(
                new
                {
                    mensaje =
                        "Hay datos inválidos.",

                    errores
                }
            );
        }
    }
}