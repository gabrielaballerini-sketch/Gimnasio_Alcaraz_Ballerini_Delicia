using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.API
{
    [ApiController]
    [Route("api/planes")]
    
    [Authorize]
    
    public class PlanApiController : ControllerBase
    {
        private readonly PlanService _service;

        public PlanApiController(
            PlanService service)
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
     

        [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador")]
          
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
        public ActionResult<Plan> Obtener(
            int id)
        {
            var plan =
                _service.Obtener(id);

            if (plan == null)
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            "Plan no encontrado."
                    }
                );
            }

            return Ok(plan);
        }

            [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador,Empleado")]
       
        [HttpPost]
        public IActionResult Crear(
            [FromForm] Plan plan)
        {
            if (!ModelState.IsValid)
                return ErroresDeModelo();

            try
            {
                var creado =
                    _service.Crear(plan);

                return CreatedAtAction(
                    nameof(Obtener),
                    new
                    {
                        id = creado.IdPlan
                    },
                    creado
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


            [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador,Empleado")]

        [HttpPut("{id:int}")]
        public IActionResult Modificar(
            int id,
            [FromForm] Plan plan)
        {
            if (!ModelState.IsValid)
                return ErroresDeModelo();

            try
            {
                var modificado =
                    _service.Modificar(
                        id,
                        plan
                    );

                if (modificado == null)
                {
                    return NotFound(
                        new
                        {
                            mensaje =
                                "Plan no encontrado."
                        }
                    );
                }

                return Ok(modificado);
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


            [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
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
                            "Plan no encontrado."
                    }
                );
            }

            return NoContent();
        }



            [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
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
                            "Plan no encontrado o ya está activo."
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