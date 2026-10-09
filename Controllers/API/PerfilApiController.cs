using System.Security.Claims;
using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.API
{
    [ApiController]
    [Route("api/perfil")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PerfilApiController : ControllerBase
    {
        private readonly UsuarioService _service;

        public PerfilApiController(UsuarioService service)
        {
            _service = service;
        }

        // Datos que manda el front en el PUT (JSON)
        public class PerfilRequest
        {
            public string? Email { get; set; }
            public string? NombreUsuario { get; set; }
            public string? PasswordActual { get; set; }
            public string? NuevaPassword { get; set; }
            public string? ConfirmarPassword { get; set; }
        }

        // GET api/perfil  mis datos
        [HttpGet]
        public IActionResult Obtener()
        {
            var id = IdLogueado();
            if (id == null) return Unauthorized();

            var usuario = _service.Obtener(id.Value);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado." });

            return Ok(Respuesta(usuario));
        }

        // PUT api/perfil  email, nombre de usuario y contraseña
        [HttpPut]
        public IActionResult Modificar([FromBody] PerfilRequest datos)
        {
            var id = IdLogueado();
            if (id == null) return Unauthorized();

            try
            {
                var usuario = _service.ActualizarPerfil(
                    id.Value,
                    datos.Email,
                    datos.NombreUsuario,
                    datos.PasswordActual,
                    datos.NuevaPassword,
                    datos.ConfirmarPassword);

                if (usuario == null)
                    return NotFound(new { mensaje = "Usuario no encontrado." });

                return Ok(Respuesta(usuario));
            }
            catch (ReglaNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // PUT api/perfil/avatar  imagen
        [HttpPut("avatar")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ActualizarAvatar(
            [FromForm] IFormFile? avatarFile)
        {
            var id = IdLogueado();
            if (id == null) return Unauthorized();

            if (avatarFile == null)
                return BadRequest(new { mensaje = "Debe seleccionar una imagen." });

            try
            {
                var usuario = await _service.ActualizarAvatar(id.Value, avatarFile);

                if (usuario == null)
                    return NotFound(new { mensaje = "Usuario no encontrado." });

                return Ok(Respuesta(usuario));
            }
            catch (ReglaNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        private int? IdLogueado()
        {
            var texto = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(texto, out var id) ? id : null;
        }

        // Nunca devolver la contraseña
        private static object Respuesta(Usuario u)
        {
            return new
            {
                u.IdUsuario,
                u.Email,
                u.NombreUsuario,
                u.Avatar
            };
        }
    }
}