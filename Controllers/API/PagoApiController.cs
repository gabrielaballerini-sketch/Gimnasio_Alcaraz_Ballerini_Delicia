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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrador,Empleado")]
    public class PagoApiController : ControllerBase
    {
        private readonly PagoService _service;
        public PagoApiController(PagoService service) => _service = service;

        [HttpGet("membresia-vigente")]
        public IActionResult BuscarMembresiaVigente([FromQuery] string dni)
        {
            try
            {
                var m = _service.ObtenerMembresiaVigentePorDni(dni);
                return Ok(new
                {
                    socio = new { m.Socio.IdSocio, m.Socio.Nombre, m.Socio.Apellido, m.Socio.Dni },
                    membresia = new
                    {
                        m.IdMembresia,
                        m.FechaInicio,
                        m.FechaFin,
                        m.PrecioContratado,
                        plan = m.Plan.Nombre
                    }
                });
            }
            catch (ReglaNegocioException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Registrar([FromBody] RegistrarPagoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { mensaje = "Revisá el DNI y el medio de pago ingresados." });

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var idUsuario))
                return Unauthorized(new { mensaje = "Token inválido: no se pudo identificar al usuario." });

            try
            {
                var pago = _service.Registrar(request.Dni, request.MedioPago, idUsuario);
                return Ok(new
                {
                    mensaje = "Pago registrado correctamente.",
                    pago.IdPago,
                    pago.Fecha,
                    pago.Importe,
                    medioPago = (int)pago.MedioPago,
                    pago.IdMembresia
                });
            }
            catch (ReglaNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }

    public class RegistrarPagoRequest
    {
        [Required]
        [RegularExpression(@"^\d{7,10}$", ErrorMessage = "El DNI debe tener entre 7 y 10 dígitos.")]
        public string Dni { get; set; } = string.Empty;

        [Range(1, 4, ErrorMessage = "Seleccioná un medio de pago válido.")]
        public MedioPago MedioPago { get; set; } = MedioPago.Efectivo;
    }
}
