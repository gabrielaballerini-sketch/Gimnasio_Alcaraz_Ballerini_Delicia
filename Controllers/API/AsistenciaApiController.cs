

using Gimnasio_Alcaraz_Ballerini_Delicia.DAO;
using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.API
{
    [ApiController]
    [Route("api/asistencias")]
    public class AsistenciaApiController : ControllerBase
    {
        private readonly AsistenciaService _service;
        private readonly IRepositorioSocio _repoSocio;
        private readonly IRepositorioClase _repoClase;
        public AsistenciaApiController(
            AsistenciaService service,
            IRepositorioSocio repoSocio,
            IRepositorioClase repoClase)
        {
            _service = service;
            _repoSocio = repoSocio;
            _repoClase = repoClase;
        }

        // GET api/asistencias/acceso/12345678
        // Consulta si un socio puede ingresar usando su DNI.
        [AllowAnonymous]
        [HttpGet("acceso/{dni}")]
        public IActionResult ConsultarAcceso(string dni)
        {


            var socio = _repoSocio.ObtenerPorDni(dni);

            if (socio == null)
            {
                return NotFound(new
                {
                    mensaje = "No existe un socio con ese DNI."
                });
            }

            try
            {
                var acceso = _service.ObtenerAcceso(socio.IdSocio);

                return Ok(acceso);
            }
            catch (ReglaNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // POST api/asistencias/registrar
        // Permite registrar musculación o una clase.
        [AllowAnonymous]
        [HttpPost("registrar")]
        public IActionResult Registrar([FromBody] RegistrarAsistencia request)
        {
            if (!ModelState.IsValid)
                return ErroresDeModelo();



            var socio = _repoSocio.ObtenerPorDni(request.Dni);

            if (socio == null)
            {
                return NotFound(new
                {
                    mensaje = "No existe un socio con ese DNI."
                });
            }

            // Una asistencia de clase debe tener IdClase.
            // Una asistencia de musculación no debe tenerlo.
            if (request.Tipo == TipoAsistencia.Clase &&
                !request.IdClase.HasValue)
            {
                return BadRequest(new
                {
                    mensaje = "Seleccioná la clase para registrar el ingreso."
                });
            }

            if (request.Tipo == TipoAsistencia.Musculacion &&
                request.IdClase.HasValue)
            {
                return BadRequest(new
                {
                    mensaje = "La asistencia de musculación no debe incluir una clase."
                });
            }

            try
            {
                var asistencia = _service.Registrar(
                    socio.IdSocio,
                    request.IdClase,
                    idUsuario: 0);

                return Ok(new
                {
                    mensaje = "Asistencia registrada correctamente.",
                    asistencia
                });
            }
            catch (ReglaNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // GET api/asistencias?desde=2026-10-01&hasta=2026-10-31&idSocio=1&pagina=1&tamPagina=10
        [Authorize(Roles = "Administrador,Empleado")]
        [HttpGet]
        public IActionResult Listar(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] int? idSocio,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamPagina = 10)
        {
            if (desde.HasValue && hasta.HasValue &&
                desde.Value.Date > hasta.Value.Date)
            {
                return BadRequest(new
                {
                    mensaje = "La fecha inicial no puede ser posterior a la fecha final."
                });
            }

            if (idSocio.HasValue && idSocio.Value <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "El ID del socio no es válido."
                });
            }

            if (pagina < 1)
                pagina = 1;

            if (tamPagina < 1)
                tamPagina = 10;

            if (tamPagina > 100)
                tamPagina = 100;

            var resultado = _service.Listar(
                desde, hasta, idSocio, pagina, tamPagina);

            return Ok(resultado);
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

        [AllowAnonymous]
        [HttpGet("clases-disponibles")]
        public IActionResult ClasesDisponibles()
        {
            var ahora = DateTime.Now;

            var hoy = (DiaSemana)(((int)ahora.DayOfWeek + 6) % 7 + 1);

            var clases = _repoClase.ObtenerGrilla()
                .Where(c =>
                    c.Estado &&
                    c.Dia == hoy &&
                    c.HoraInicio <= ahora.TimeOfDay &&
                    c.HoraFin > ahora.TimeOfDay &&
                    c.Actividad != null &&
                    !c.Actividad.AccesoLibre)
                .OrderBy(c => c.HoraInicio)
                .Select(c => new
                {
                    idClase = c.IdClase,
                    actividad = c.Actividad.Nombre,
                    profesor = c.Profesor.Nombre + " " + c.Profesor.Apellido,
                    horaInicio = c.HoraInicio.ToString(@"hh\:mm"),
                    horaFin = c.HoraFin.ToString(@"hh\:mm")
                })
                .ToList();

            return Ok(clases);
        }

    }


}
