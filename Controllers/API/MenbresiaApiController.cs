using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.Service;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost]
        public IActionResult Crear(
            [FromForm] Membresia membresia)
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

            try
            {
                var creada =
                    _service.Crear(membresia);

                return CreatedAtAction(
                    nameof(Obtener),
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

        [HttpPut("{id:int}")]
        public IActionResult Modificar(
            int id,
            [FromForm] Membresia membresia)
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
    }
}