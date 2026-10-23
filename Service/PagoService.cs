using Gimnasio_Alcaraz_Ballerini_Delicia.DAO;
using Gimnasio_Alcaraz_Ballerini_Delicia.Models;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Service
{
    public class PagoService
    {
        private readonly IRepositorioPago _repoPago;
        private readonly IRepositorioMembresia _repoMembresia;

        public PagoService(IRepositorioPago repoPago, IRepositorioMembresia repoMembresia)
        {
            _repoPago = repoPago;
            _repoMembresia = repoMembresia;
        }

        public Membresia ObtenerMembresiaVigentePorDni(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni) || !System.Text.RegularExpressions.Regex.IsMatch(dni.Trim(), @"^\d{7,10}$"))
                throw new ReglaNegocioException("Ingresá un DNI válido de entre 7 y 10 dígitos.");

            var membresia = _repoMembresia.ObtenerVigentePorDni(dni.Trim(), DateTime.Today);
            if (membresia == null)
                throw new ReglaNegocioException("No se encontró un socio activo con una membresía vigente para ese DNI.");

            return membresia;
        }

        public Pago Registrar(string dni, MedioPago medioPago, int idUsuarioCreador)
        {
            if (!Enum.IsDefined(typeof(MedioPago), medioPago))
                throw new ReglaNegocioException("Seleccioná un medio de pago válido.");
            if (idUsuarioCreador <= 0)
                throw new ReglaNegocioException("No se pudo identificar al usuario que registra el pago.");

            var membresia = ObtenerMembresiaVigentePorDni(dni);
            if (membresia.PrecioContratado <= 0)
                throw new ReglaNegocioException("La membresía tiene un importe contratado inválido.");

            var pago = new Pago
            {
                IdMembresia = membresia.IdMembresia,
                Fecha = DateTime.Now,
                Importe = membresia.PrecioContratado,
                MedioPago = medioPago,
                Estado = true,
                IdUsuarioCreador = idUsuarioCreador
            };

            _repoPago.Alta(pago);
            return pago;
        }
    }
}
