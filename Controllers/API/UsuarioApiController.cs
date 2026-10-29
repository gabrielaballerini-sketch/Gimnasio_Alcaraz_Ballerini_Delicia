
using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.API
{
    [ApiController]
    [Route("api/usuarios")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
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
            

            return Ok(_service.ObtenerActivos(pagina, tamPagina));
        }

        [HttpGet("inactivos")]
        public IActionResult ObtenerInactivos(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamPagina = 10)
        {
           

            return Ok(_service.ObtenerInactivos(pagina, tamPagina));
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
            // En edición la contraseña es opcional: si viene vacía se conserva la actual
            if (string.IsNullOrWhiteSpace(usuario.Password))
                ModelState.Remove(nameof(Usuario.Password));

          

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
        [RequestSizeLimit(6 * 1024 * 1024)]
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
        // Evita devolver la contraseña en las respuestas de la API.
        private static object RespuestaUsuario(Usuario usuario)
        {
            return new
            {
                usuario.IdUsuario,
                usuario.Email,
                usuario.NombreUsuario,
                usuario.Roles,
                usuario.Estado,
                usuario.Avatar,
                usuario.IdSocio,
                Socio = usuario.Socio == null ? null : new
                {
                    usuario.Socio.IdSocio,
                    usuario.Socio.Nombre,
                    usuario.Socio.Apellido,
                    usuario.Socio.Dni
                }
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


[HttpPut("{id:int}/convertir-a-socio")]
public IActionResult ConvertirASocio(
    int id,
    [FromBody] Socio? datos)
{
    // Sin ModelState: si el usuario ya tuvo ficha no hace falta enviar datos.
    // El servicio valida los datos solo cuando hay que crear la ficha.
    try
    {
        var usuario = _service.ConvertirASocio(id, datos ?? new Socio());

        if (usuario == null)
        {
            return NotFound(new
            {
                mensaje = "Usuario no encontrado."
            });
        }

        return Ok(RespuestaUsuario(usuario));
    }
    catch (ReglaNegocioException ex)
    {
        return BadRequest(new { mensaje = ex.Message, errores = ex.Message });
    }
}

    }
}


