using Gimnasio_Alcaraz_Ballerini_Delicia.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.API
{
    [ApiController]
    [Route("api/actividades")]
    public class ActividadArchivoApiController : ControllerBase
    {
        private readonly ActividadArchivoService _service;

        public ActividadArchivoApiController(
            ActividadArchivoService service)
        {
            _service = service;
        }


        // LISTAR las fotos y videos de una actividad 
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{idActividad:int}/archivos")]
        public IActionResult Listar(int idActividad)
        {
            var archivos = _service.Listar(idActividad);

            if (archivos == null)
            {
                return NotFound(new
                {
                    mensaje = "Actividad no encontrada."
                });
            }

            return Ok(archivos);
        }


        // SUBIR una foto o video
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
            Roles = "Administrador,Empleado")]
        
        [HttpPost("{idActividad:int}/archivos")]
        
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(25 * 1024 * 1024)]
        public async Task<IActionResult> Subir(
            int idActividad,
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
                var creado = await _service.Subir(idActividad, archivo);

                if (creado == null)
                {
                    return NotFound(new
                    {
                        mensaje = "Actividad no encontrada."
                    });
                }

                return Ok(creado);
            }
            catch (ReglaNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }


        // ELIMINAR una foto o video
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
            Roles = "Administrador,Empleado")]
        
        [HttpDelete("archivos/{idArchivo:int}")]
        
        public IActionResult Eliminar(int idArchivo)
        {
            if (!_service.Eliminar(idArchivo))
            {
                return NotFound(new
                {
                    mensaje = "Archivo no encontrado."
                });
            }

            return NoContent();
        }
    }
}
