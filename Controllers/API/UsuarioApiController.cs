
using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.API
{
    [ApiController]
    [Route("api/usuarios")]
    [Authorize(
        AuthenticationSchemes =
            JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador")]
    public class UsuarioApiController : ControllerBase
    {
        private readonly UsuarioService _service;

        public UsuarioApiController(UsuarioService service)
        {
            _service = service;
        }

        [HttpGet("activos")]
        public IActionResult ObtenerActivos(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamPagina = 10)
        {
            var resultado = _service.ObtenerActivos(
                pagina,
                tamPagina
            );

            return Ok(new
            {
                resultado.Pagina,
                resultado.TamPagina,
                resultado.Total,
                Items = resultado.Items.Select(RespuestaUsuario)
            });
        }

        [HttpGet("inactivos")]
        public IActionResult ObtenerInactivos(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamPagina = 10)
        {
            var resultado = _service.ObtenerInactivos(
                pagina,
                tamPagina
            );

            return Ok(new
            {
                resultado.Pagina,
                resultado.TamPagina,
                resultado.Total,
                Items = resultado.Items.Select(RespuestaUsuario)
            });
        }

        [HttpGet("{id:int}")]
        public IActionResult Obtener(int id)
        {
            var usuario = _service.Obtener(id);

            if (usuario == null)
            {
                return NotFound(new
                {
                    mensaje = "Usuario no encontrado."
                });
            }

            return Ok(RespuestaUsuario(usuario));
        }

        [HttpPost]
        public IActionResult Crear([FromBody] Usuario usuario)
        {
            if (!ModelState.IsValid)
                return ErroresDeModelo();

            try
            {
                var creado = _service.Crear(usuario);

                return CreatedAtAction(
                    nameof(Obtener),
                    new { id = creado.IdUsuario },
                    RespuestaUsuario(creado)
                );
            }
            catch (ReglaNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
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
                var modificado = _service.Modificar(id, usuario);

                if (modificado == null)
                {
                    return NotFound(new
                    {
                        mensaje = "Usuario no encontrado."
                    });
                }

                return Ok(RespuestaUsuario(modificado));
            }
            catch (ReglaNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // SUBIR O CAMBIAR AVATAR
        [HttpPut("{id:int}/avatar")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ActualizarAvatar(
            int id,
            [FromForm] IFormFile? avatarFile)
        {
            if (avatarFile == null)
            {
                return BadRequest(new
                {
                    mensaje = "Debe seleccionar una imagen."
                });
            }

            try
            {
                var usuario = await _service.ActualizarAvatar(
                    id,
                    avatarFile
                );

                if (usuario == null)
                {
                    return NotFound(new
                    {
                        mensaje = "Usuario no encontrado."
                    });
                }

                return Ok(new
                {
                    mensaje = "Avatar actualizado correctamente.",
                    usuario = RespuestaUsuario(usuario)
                });
            }
            catch (ReglaNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public IActionResult DarDeBaja(int id)
        {
            if (!_service.DarDeBaja(id))
            {
                return NotFound(new
                {
                    mensaje = "Usuario no encontrado."
                });
            }

            return NoContent();
        }

        [HttpPut("{id:int}/reactivar")]
        public IActionResult Reactivar(int id)
        {
            if (!_service.Reactivar(id))
            {
                return NotFound(new
                {
                    mensaje =
                        "Usuario no encontrado o ya está activo."
                });
            }

            return NoContent();
        }

        // Evita devolver la contraseña en las respuestas de la API.
        private static object RespuestaUsuario(Usuario usuario)
        {
            return new
            {
                usuario.IdUsuario,
                usuario.Email,
                usuario.Roles,
                usuario.Estado,
                usuario.Avatar
            };
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
