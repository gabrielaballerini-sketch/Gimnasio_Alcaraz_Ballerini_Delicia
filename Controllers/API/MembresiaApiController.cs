using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.API
{
    [ApiController]
    [Route("api/membresias")]
    public class MembresiaApiController : ControllerBase
    {
        private readonly MembresiaService _service;

        public MembresiaApiController(
            MembresiaService service)
        {
            _service = service;
        }

        // MI MEMBRESÍA: el socio ve solo la suya. El IdSocio sale del token, no de la URL.
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Socio")]
        [HttpGet("mia")]
        public IActionResult ObtenerMia()
        {
            if (!int.TryParse(User.FindFirst("IdSocio")?.Value, out var idSocio))
            {
                return NotFound(new
                {
                    mensaje = "Tu usuario no tiene una ficha de socio asociada."
                });
            }

            var membresias = _service
                .ObtenerActivosPorSocio(idSocio, 1, 50)
                .Select(m => new
                {
                    m.IdMembresia,
                    m.FechaInicio,
                    m.FechaFin,
                    m.PrecioContratado,
                    m.Pagada,
                    Plan = new
                    {
                        m.Plan.IdPlan,
                        m.Plan.Nombre,
                        m.Plan.UtilizacionesMensuales
                    }
                });

            return Ok(membresias);
        }


        // CAMBIO: antes lo podía llamar cualquier usuario logueado (un socio veía las membresías de todos)
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador, Empleado")]
        [HttpGet("activos")]
        public IActionResult ObtenerActivos(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamPagina = 10,
            [FromQuery] string? dni = null)
        {
            // Con DNI: busca por DNI en vez de listar todo.
            if (!string.IsNullOrWhiteSpace(dni))
            {
                return Ok(_service.BuscarPorDni(dni, true, pagina, tamPagina));
            }

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
            [FromQuery] int tamPagina = 10,
            [FromQuery] string? dni = null)
        {
            // Con DNI: busca por DNI en vez de listar todo.
            if (!string.IsNullOrWhiteSpace(dni))
            {
                return Ok(_service.BuscarPorDni(dni, false, pagina, tamPagina));
            }

            return Ok(
                _service.ObtenerInactivos(
                    pagina,
                    tamPagina
                )
            );
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador, Empleado")]
        [HttpGet("{id:int}")]
        public ActionResult<Membresia> Obtener(
            int id)
        {
            var membresia =
                _service.Obtener(id);

            if (membresia == null)
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            "Membresía no encontrada."
                    }
                );
            }

            return Ok(membresia);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador, Empleado")]
        [HttpGet("socio/{idSocio:int}")]
        public IActionResult ObtenerPorSocio(
            int idSocio,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamPagina = 10)
        {
            return Ok(
                _service.ObtenerActivosPorSocio(
                    idSocio,
                    pagina,
                    tamPagina
                )
            );
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador, Empleado")]
        [HttpPost]
        public IActionResult Crear( [FromForm] Membresia membresia)
        {
            // Las propiedades de navegación no vienen del formulario.
            // Solo necesitamos los Id de Socio y Plan.
            foreach (var key in ModelState.Keys
                .Where(k =>
                    k.StartsWith("Socio") ||
                    k.StartsWith("Plan") ||
                    k.StartsWith("Usuario"))
                .ToList())
            {
                ModelState.Remove(key);
            }
            if (!ModelState.IsValid)
                return ErroresDeModelo();
            // El creador sale del token, nunca del formulario
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var idUsuario))
                return Unauthorized(new { mensaje = "Token inválido." });

            try
            {
                var creada =
                    _service.Crear(membresia,idUsuario);

                return CreatedAtAction( nameof(Obtener),
                    new
                    {
                        id = creada.IdMembresia
                    },
                    creada
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
        Roles = "Administrador, Empleado")]
        [HttpPut("{id:int}")]
        public IActionResult Modificar(
            int id,
            [FromForm] Membresia membresia)

    {
        // Las propiedades de navegación no vienen del formulario.
    // Solo recibimos los identificadores del socio y del plan.
    foreach (var key in ModelState.Keys
        .Where(k =>
            k.StartsWith("Socio") ||
            k.StartsWith("Plan") ||
            k.StartsWith("Usuario"))
        .ToList())
    {
        ModelState.Remove(key);
    }




        {
            if (!ModelState.IsValid)
                return ErroresDeModelo();

            try
            {
                var modificada =
                    _service.Modificar(
                        id,
                        membresia
                    );

                if (modificada == null)
                {
                    return NotFound(
                        new
                        {
                            mensaje =
                                "Membresía no encontrada."
                        }
                    );
                }

                return Ok(modificada);
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
                            "Membresía no encontrada."
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
                            "Membresía no encontrada o ya está activa."
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


[Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Administrador")]
[HttpGet("proximas-vencer")]
public IActionResult ObtenerProximasAVencer(
    [FromQuery] int dias = 15,
    [FromQuery] int pagina = 1,
    [FromQuery] int tamPagina = 10)
{
    try
    {
        var resultado = _service.ObtenerProximasAVencer(
            dias,
            pagina,
            tamPagina
        );

        return Ok(new
        {
            items = resultado.Items,
            pagina = resultado.Pagina,
            tamPagina = resultado.TamPagina,
            total = resultado.Total,
            totalPaginas = resultado.TotalPaginas
        });
    }
    catch (ReglaNegocioException ex)
    {
        return BadRequest(new
        {
            mensaje = ex.Message
        });
    }
}


[Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Administrador")]
[HttpGet("vigentes-en-fecha")]
public IActionResult ObtenerVigentesEnFecha(
    [FromQuery] DateTime? fecha,
    [FromQuery] int pagina = 1,
    [FromQuery] int tamPagina = 10)
{
    if (!fecha.HasValue)
    {
        return BadRequest(new
        {
            mensaje = "Debe seleccionar una fecha."
        });
    }

    try
    {
        var resultado = _service.ObtenerVigentesEnFecha(
            fecha.Value,
            pagina,
            tamPagina
        );

        return Ok(new
        {
            items = resultado.Items,
            pagina = resultado.Pagina,
            tamPagina = resultado.TamPagina,
            total = resultado.Total,
            totalPaginas = resultado.TotalPaginas
        });
    }
    catch (ReglaNegocioException ex)
    {
        return BadRequest(new
        {
            mensaje = ex.Message
        });
    }
}

    }
}