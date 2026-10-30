

using Gimnasio_Alcaraz_Ballerini_Delicia.DAO;
using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

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

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador")]
        [HttpGet]
        public IActionResult Listar(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] int? idSocio,
            [FromQuery] string? dni,
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

            // Búsqueda por DNI: se resuelve al socio y se filtra por su id.
            // No se exige que esté activo para poder consultar su historial.
            if (!string.IsNullOrWhiteSpace(dni))
            {
                dni = dni.Trim();

                if (!Regex.IsMatch(dni, @"^\d{7,10}$"))
                {
                    return BadRequest(new
                    {
                        mensaje = "El DNI debe tener entre 7 y 10 dígitos."
                    });
                }

                var socio = _repoSocio.ObtenerPorDni(dni);

                if (socio == null)
                {
                    return NotFound(new
                    {
                        mensaje = "No existe un socio con ese DNI."
                    });
                }

                idSocio = socio.IdSocio;
            }

            if (idSocio.HasValue && idSocio.Value <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "El ID del socio no es válido."
                });
            }

            (pagina, tamPagina) =PaginacionParametros.Normalizar(pagina, tamPagina);


            var resultado = _service.Listar(
                desde, hasta, idSocio, pagina, tamPagina);

            return Ok(resultado);
        }



        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Socio")]
        [HttpGet("mia")]
        public IActionResult ObtenerMias(
                  [FromQuery] DateTime? desde,
                  [FromQuery] DateTime? hasta,
                  [FromQuery] bool periodoActual = true,
                  [FromQuery] int pagina = 1,
                  [FromQuery] int tamPagina = 10)
        {
            if (!int.TryParse(User.FindFirst("IdSocio")?.Value, out var idSocio))
            {
                return NotFound(new
                {
                    mensaje = "Tu usuario no tiene una ficha de socio asociada."
                });
            }

            if (desde.HasValue && hasta.HasValue &&
                desde.Value.Date > hasta.Value.Date)
            {
                return BadRequest(new
                {
                    mensaje = "La fecha inicial no puede ser posterior a la fecha final."
                });
            }
                (pagina, tamPagina) =PaginacionParametros.Normalizar(pagina, tamPagina);


            try
            {
                var acceso = _service.ObtenerAcceso(idSocio);

                var resumen = new
                {
                    plan = acceso.Plan,
                    tienePeriodoVigente = acceso.IdMembresia.HasValue,
                    vigenteDesde = acceso.VigenteDesde,
                    venceEl = acceso.VenceEl,
                    ilimitado = acceso.Ilimitado,
                    permitidas = acceso.Limite,
                    utilizadas = acceso.Usadas,
                    disponibles = acceso.Ilimitado
                        ? (int?)null
                        : Math.Max(0, (acceso.Limite ?? 0) - acceso.Usadas),
                    motivo = acceso.Motivo
                };

                if (periodoActual)
                {
                    // Sin período vigente (sin membresía o pendiente de pago)
                    // no hay nada que mostrar en "período actual".
                    if (!acceso.VigenteDesde.HasValue || !acceso.VenceEl.HasValue)
                    {
                        return Ok(new
                        {
                            resumen,
                            items = new List<Asistencia>(),
                            total = 0,
                            pagina = 1,
                            tamPagina
                        });
                    }

                    // Las fechas las fija el servidor, no el navegador
                    desde = acceso.VigenteDesde.Value.Date;
                    hasta = acceso.VenceEl.Value.Date;
                }

                var historial = _service.Listar(desde, hasta, idSocio, pagina, tamPagina);

                return Ok(new
                {
                    resumen,
                    items = historial.Items,
                    total = historial.Total,
                    pagina = historial.Pagina,
                    tamPagina = historial.TamPagina
                });
            }
            catch (ReglaNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
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
                    AsistenciaService.ClaseHabilitadaParaIngreso(c, ahora.TimeOfDay) &&
                    c.Actividad != null &&
                    !c.Actividad.AccesoLibre)
                .OrderBy(c => c.HoraInicio)
                .Select(c => new
                {
                    idClase = c.IdClase,
                    actividad = c.Actividad.Nombre,
                    profesor = c.Profesor.Nombre + " " + c.Profesor.Apellido,
                    horaInicio = c.HoraInicio.ToString(@"hh\:mm"),
                    horaFin = c.HoraFin.ToString(@"hh\:mm"),

                    // Para que en pantalla se vea si ya empezó o cuánto falta.
                    enCurso = c.HoraInicio <= ahora.TimeOfDay,
                    minutosParaInicio = c.HoraInicio <= ahora.TimeOfDay
                        ? 0
                        : (int)Math.Ceiling((c.HoraInicio - ahora.TimeOfDay).TotalMinutes)
                })
                .ToList();

            return Ok(clases);
        }

    }


}
