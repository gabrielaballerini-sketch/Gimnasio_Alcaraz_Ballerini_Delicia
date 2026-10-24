
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
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador,Empleado")]
    public class PagoApiController : ControllerBase
    {
        private readonly PagoService _service;

        public PagoApiController(PagoService service)
        {
            _service = service;
        }

        // Busca la membresía del socio que corresponde cobrar
        // (pendiente de pago, vencida o vigente).
        [HttpGet("membresia-cobro")]
        public IActionResult BuscarMembresiaParaCobro(
            [FromQuery] string dni)
        {
            try
            {
                var membresia =
                    _service.ObtenerMembresiaParaCobroPorDni(dni);

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
    }

    public class RegistrarPagoMembresiaRequest
    {
        [Range(1, 4, ErrorMessage = "Seleccioná un medio de pago válido.")]
        public MedioPago MedioPago { get; set; } = MedioPago.Efectivo;
    }
}
