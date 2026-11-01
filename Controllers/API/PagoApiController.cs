using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.API
{
    [ApiController]
    [Route("api/pagos")]
    public class PagoApiController : ControllerBase
    {
        private readonly PagoService _service;
        private readonly SocioService _socioService;

        public PagoApiController(PagoService service, SocioService socioService)
        {
            _service = service;
            _socioService = socioService;
        }

        // Busca la membresía del socio que corresponde cobrar
        // (pendiente de pago, vencida o vigente).
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
            Roles = "Administrador,Empleado")]
        [HttpGet("membresia-cobro")]
        public IActionResult BuscarMembresiaParaCobro([FromQuery] string dni)
        {
            try
            {
                var membresia = _service.ObtenerMembresiaParaCobroPorDni(dni);

                var hoy = DateTime.Today;
                bool vigente =
                    membresia.Pagada &&
                    membresia.FechaInicio.Date <= hoy &&
                    membresia.FechaFin.Date >= hoy;

                return Ok(new
                {
                    socio = new
                    {
                        membresia.Socio.IdSocio,
                        membresia.Socio.Nombre,
                        membresia.Socio.Apellido,
                        membresia.Socio.Dni
                    },

                    membresia = new
                    {
                        membresia.IdMembresia,
                        membresia.FechaInicio,
                        membresia.FechaFin,
                        membresia.PrecioContratado,
                        membresia.Pagada,
                        vigente,
                        requiereCobro = !vigente,
                        importeACobrar = vigente
                            ? membresia.PrecioContratado
                            : _service.ObtenerImporteACobrar(membresia),
                        plan = membresia.Plan.Nombre
                    }
                });
            }
            catch (ReglaNegocioException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        // Registra el pago inicial o la renovación.
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
            Roles = "Administrador,Empleado")]
        [HttpPost("membresia/{idMembresia:int}")]
        public IActionResult RegistrarPorMembresia(
            int idMembresia,
            [FromBody] RegistrarPagoMembresiaRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    mensaje = "Seleccioná un medio de pago válido."
                });
            }

            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var idUsuario))
            {
                return Unauthorized(new
                {
                    mensaje = "Token inválido: no se pudo identificar al usuario."
                });
            }

            try
            {
                var pago = _service.Registrar(
                    idMembresia,
                    request.MedioPago,
                    idUsuario);

                return Ok(new
                {
                    mensaje = "Pago registrado correctamente.",
                    pago.IdPago,
                    pago.IdMembresia,
                    pago.Fecha,
                    pago.Importe,
                    medioPago = (int)pago.MedioPago
                });
            }
            catch (ReglaNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // Listado de todos los pagos
        // Quién registr / anuló el pago 

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
            Roles = "Administrador")]
        [HttpGet]
        public IActionResult Listar(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamPagina = 10)
        {
            (pagina, tamPagina) = PaginacionParametros.Normalizar(pagina, tamPagina);


            var resultado = _service.Listar(pagina, tamPagina);
            bool esAdmin = User.IsInRole("Administrador");

            return Ok(new
            {
                items = resultado.Items.Select(p => Proyectar(p, esAdmin)),
                total = resultado.Total,
                pagina = resultado.Pagina,
                tamPagina = resultado.TamPagina
            });
        }

        // Pagos del socio logueado. 
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
            Roles = "Socio")]
        [HttpGet("mios")]
        public IActionResult Mios(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamPagina = 10)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var idUsuario))
            {
                return Unauthorized(new { mensaje = "Token inválido." });
            }
            (pagina, tamPagina) = PaginacionParametros.Normalizar(pagina, tamPagina);


            var pagos = _service.ListarDeUsuario(idUsuario, pagina, tamPagina);

            return Ok(pagos.Select(p => Proyectar(p, false)));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
            Roles = "Administrador")]
        [HttpPost("{idPago:int}/anular")]
        public IActionResult Anular(int idPago)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var idUsuario))
            {
                return Unauthorized(new
                {
                    mensaje = "Token inválido: no se pudo identificar al usuario."
                });
            }

            try
            {
                _service.Anular(idPago, idUsuario);

                return Ok(new { mensaje = "Pago anulado correctamente." });
            }
            catch (ReglaNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

    // RECAUDACIÓN POR PERÍODO

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Administrador")]

[HttpGet("recaudacion")]

public IActionResult Recaudacion(
    [FromQuery] DateTime? desde,
    [FromQuery] DateTime? hasta,
    [FromQuery] int? idPlan,
    [FromQuery] int? medioPago)
{
    try
    {
        var r = _service.Recaudacion(desde, hasta, idPlan, medioPago);

        // El resumen sale de sumar el desglose por plan (mismos filtros)
        var total = r.PorPlan.Sum(x => x.Total);
        var cantidad = r.PorPlan.Sum(x => x.Cantidad);

        return Ok(new
        {
            resumen = new
            {
                total,
                cantidad,
                ticketPromedio = cantidad > 0 ? Math.Round(total / cantidad, 2) : 0m
            },
            porPlan = r.PorPlan.Select(x => new
            {
                x.Plan,
                x.Cantidad,
                x.Total,
                porcentaje = total > 0 ? Math.Round(x.Total * 100m / total, 1) : 0m
            }),
            porMedio = r.PorMedio.Select(x => new
            {
                x.Medio,
                x.Cantidad,
                x.Total,
                porcentaje = total > 0 ? Math.Round(x.Total * 100m / total, 1) : 0m
            })
        });
    }
    catch (ReglaNegocioException ex)
    {
        return BadRequest(new { mensaje = ex.Message });
    }
}

// PAGOS DE UN SOCIO (administrador )

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Administrador")]

[HttpGet("socio/{idSocio:int}")]
public IActionResult DeSocio(
    int idSocio,
    [FromQuery] int pagina = 1,
    [FromQuery] int tamPagina = 10)
{
    var socio = _socioService.Obtener(idSocio);

    if (socio == null)
        return NotFound(new { mensaje = "Socio no encontrado." });

    (pagina, tamPagina) = PaginacionParametros.Normalizar(pagina, tamPagina);

    if (!socio.IdUsuario.HasValue)
        return Ok(new { items = Array.Empty<object>(), total = 0, pagina, tamPagina });

    var pagos = _service.ListarDeUsuario(socio.IdUsuario.Value, pagina, tamPagina);

    return Ok(new
    {
        items = pagos.Select(p => Proyectar(p, false)),
        total = _service.ContarDeUsuario(socio.IdUsuario.Value),
        pagina,
        tamPagina
    });
}




        // lo usamos en listar y mio ( para q salga solo lo q yo quiero)
        private static object Proyectar(Pago p, bool incluirAuditoria)
        {
            return new
            {
                p.IdPago,
                p.IdMembresia,
                p.Fecha,
                p.Importe,
                medioPago = (int)p.MedioPago,
                estado = p.Estado,
                socio = p.Membresia?.Socio == null ? null : new
                {
                    p.Membresia.Socio.IdSocio,
                    p.Membresia.Socio.Nombre,
                    p.Membresia.Socio.Apellido
                },
                idUsuarioCreador = incluirAuditoria ? (int?)p.IdUsuarioCreador : null,
                idUsuarioAnulador = incluirAuditoria ? p.IdUsuarioAnulador : null
            };
        }
    }

    public class RegistrarPagoMembresiaRequest
    {
        [Range(1, 4, ErrorMessage = "Seleccioná un medio de pago válido.")]
        public MedioPago MedioPago { get; set; } = MedioPago.Efectivo;
    }
}