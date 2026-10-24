using Gimnasio_Alcaraz_Ballerini_Delicia.DAO;
using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using System.Text.RegularExpressions;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Service
{
    public class PagoService
    {
        private readonly IRepositorioPago _repoPago;
        private readonly IRepositorioMembresia _repoMembresia;
        private readonly IRepositorio<Plan> _repoPlan;

        public PagoService(
            IRepositorioPago repoPago,
            IRepositorioMembresia repoMembresia,
            IRepositorio<Plan> repoPlan)
        {
            _repoPago = repoPago;
            _repoMembresia = repoMembresia;
            _repoPlan = repoPlan;
        }

   
        public static bool EsRenovacion(Membresia membresia)
        {
            return membresia.Pagada &&
                   membresia.FechaFin.Date < DateTime.Today;
        }

        // Primer pago o período pendiente: se cobra el precio guardado al contratar.
        // Renovación: es un período nuevo, se cobra el precio actual del plan.
        public decimal ObtenerImporteACobrar(Membresia membresia)
        {
            if (!EsRenovacion(membresia))
                return membresia.PrecioContratado;

            var plan = _repoPlan.ObtenerPorId(membresia.IdPlan);

            if (plan == null)
            {
                throw new ReglaNegocioException(
                    "No se encontró el plan de la membresía.");
            }

            if (!plan.Estado)
            {
                throw new ReglaNegocioException(
                    "El plan de esta membresía ya no está disponible. " +
                    "Cambiá el plan desde Membresías para poder renovarla.");
            }

            if (plan.Precio <= 0)
            {
                throw new ReglaNegocioException(
                    "El precio actual del plan es inválido.");
            }

            return plan.Precio;
        }

        public Membresia ObtenerMembresiaParaCobroPorDni(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni) ||
                !Regex.IsMatch(dni.Trim(), @"^\d{7,10}$"))
            {
                throw new ReglaNegocioException(
                    "Ingresá un DNI válido de entre 7 y 10 dígitos.");
            }

            var membresia = _repoMembresia.ObtenerParaCobroPorDni(dni.Trim());

            if (membresia == null)
            {
                throw new ReglaNegocioException(
                    "No se encontró una membresía activa para ese DNI. " +
                    "Primero hay que crearla desde Membresías.");
            }

            return membresia;
        }

        public Pago Registrar(
            int idMembresia,
            MedioPago medioPago,
            int idUsuarioCreador)
        {
            if (idMembresia <= 0)
            {
                throw new ReglaNegocioException(
                    "La membresía indicada no es válida.");
            }

            if (!Enum.IsDefined(typeof(MedioPago), medioPago))
            {
                throw new ReglaNegocioException(
                    "Seleccioná un medio de pago válido.");
            }

            if (idUsuarioCreador <= 0)
            {
                throw new ReglaNegocioException(
                    "No se pudo identificar al usuario que registra el pago.");
            }

            var membresia = _repoMembresia.ObtenerPorId(idMembresia);

            if (membresia == null)
            {
                throw new ReglaNegocioException(
                    "No se encontró la membresía.");
            }

            if (!membresia.Estado)
            {
                throw new ReglaNegocioException(
                    "No se puede cobrar una membresía dada de baja.");
            }

            var importe = ObtenerImporteACobrar(membresia);

            if (importe <= 0)
            {
                throw new ReglaNegocioException(
                    "La membresía tiene un importe inválido.");
            }

            var hoy = DateTime.Today;

            bool vigente =
                membresia.FechaInicio.Date <= hoy &&
                membresia.FechaFin.Date >= hoy;

            if (membresia.Pagada && vigente)
            {
                throw new ReglaNegocioException(
                    "La membresía ya está pagada y vigente. No se puede cobrar otra vez este período.");
            }

            DateTime fechaInicio;

            if (EsRenovacion(membresia))
            {
                // Si la membresía anterior ya venció,
                // la renovación comienza al día siguiente del vencimiento.
                fechaInicio = membresia.FechaFin.Date.AddDays(1);

                // Evita generar una renovación con inicio en el pasado
                // si el vencimiento fue hace mucho tiempo.
                if (fechaInicio < hoy)
                    fechaInicio = hoy;
            }
            else
            {
                // Primera membresía o período todavía no pagado.
                fechaInicio = hoy;
            }

            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            var pago = new Pago
            {
                IdMembresia = membresia.IdMembresia,
                Fecha = DateTime.Now,
                Importe = importe,
                MedioPago = medioPago,
                Estado = true,
                IdUsuarioCreador = idUsuarioCreador
            };

            _repoPago.RegistrarPagoYHabilitarMembresia(
                pago,
                fechaInicio,
                fechaFin);

            return pago;
        }

               public void Anular(int idPago, int idUsuarioAnulador)
        {
            if (idPago <= 0)
            {
                throw new ReglaNegocioException(
                    "El pago indicado no es válido.");
            }

            if (idUsuarioAnulador <= 0)
            {
                throw new ReglaNegocioException( "No se pudo identificar al usuario que anula el pago.");
            }

            var pago = _repoPago.ObtenerPorId(idPago) ?? throw new ReglaNegocioException("No se encontró el pago.");

            if (!pago.Estado)
            { throw new ReglaNegocioException("El pago ya está anulado.");
            }

            if (_repoPago.Anular(idPago, idUsuarioAnulador) == 0)
            {
                throw new ReglaNegocioException("No se pudo anular el pago.");
            }
        }

        // Todos los pagos (activos y anulados) para Administrador y Empleado.
        public ResultadoPaginado<Pago> Listar(int pagina = 1, int tamPagina = 10)
        {
            if (pagina < 1)
                pagina = 1;

            if (tamPagina < 1)
                tamPagina = 10;

            return new ResultadoPaginado<Pago>
            {
                Items = _repoPago.ObtenerTodos(pagina, tamPagina),
                Pagina = pagina,
                TamPagina = tamPagina,
                Total = _repoPago.ObtenerCantidad(null)   
            };
        }

        // Pagos del socio logueado (se busca por IdUsuario).
        public IList<Pago> ListarDeUsuario(
            int idUsuario,
            int pagina = 1,
            int tamPagina = 100)
        {
            if (pagina < 1)
                pagina = 1;

            if (tamPagina < 1)
                tamPagina = 10;

            return _repoPago.ObtenerPorSocio(idUsuario, pagina, tamPagina);
        }
    }
}