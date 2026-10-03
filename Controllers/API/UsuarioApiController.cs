using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.Service;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.API
{
    [ApiController]
    [Route("api/usuarios")]
    public class UsuarioApiController : ControllerBase
    {
        private readonly UsuarioService _service;

        public UsuarioApiController(
            UsuarioService service)
        {
            _service = service;
        }

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

        [HttpGet("{id:int}")]
        public ActionResult<Usuario> Obtener(
            int id)
        {
            var usuario =
                _service.Obtener(id);

            if (usuario == null)
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            "Usuario no encontrado."
                    }
                );
            }

            return Ok(usuario);
        }

        [HttpPost]
        public IActionResult Crear(
            [FromBody] Usuario usuario)
        {
            if (!ModelState.IsValid)
                return ErroresDeModelo();

            try
            {
                var creado =
                    _service.Crear(usuario);

                return CreatedAtAction(
                    nameof(Obtener),
                    new
                    {
                        id = creado.IdUsuario
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

        [HttpPut("{id:int}")]
        public IActionResult Modificar(
            int id,
            [FromBody] Usuario usuario)
        {
            if (!ModelState.IsValid)
                return ErroresDeModelo();

            try
            {
                var modificado =
                    _service.Modificar(
                        id,
                        usuario
                    );

                if (modificado == null)
                {
                    return NotFound(
                        new
                        {
                            mensaje =
                                "Usuario no encontrado."
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

        [HttpDelete("{id:int}")]
        public IActionResult DarDeBaja(int id)
        {
            if (!_service.DarDeBaja(id))
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            "Usuario no encontrado."
                    }
                );
            }

            return NoContent();
        }

        [HttpPut("{id:int}/reactivar")]
        public IActionResult Reactivar(int id)
        {
            if (!_service.Reactivar(id))
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            "Usuario no encontrado o ya está activo."
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