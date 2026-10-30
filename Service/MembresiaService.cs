
using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.DAO;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Service
{
    public class MembresiaService
    {
        private readonly IRepositorioMembresia _repo;
        private readonly IRepositorioSocio _repoSocio;
        private readonly IRepositorio<Plan> _repoPlan;

        public MembresiaService(
            IRepositorioMembresia repo,
            IRepositorioSocio repoSocio,
            IRepositorio<Plan> repoPlan)
        {
            _repo = repo;
            _repoSocio = repoSocio;
            _repoPlan = repoPlan;
        }

      

        public ResultadoPaginado<Membresia> ObtenerActivos(
            int pagina = PaginacionParametros.PaginaPredeterminada,
             int tamPagina = PaginacionParametros.TamPaginaPredeterminado)
        {
              (pagina, tamPagina) =PaginacionParametros.Normalizar(pagina, tamPagina);

            return new ResultadoPaginado<Membresia>
            {
                Items = _repo.ObtenerActivos(pagina, tamPagina),
                Pagina = pagina,
                TamPagina = tamPagina,
                Total = _repo.ObtenerCantidad(true)
            };
        }

   

        public ResultadoPaginado<Membresia> ObtenerInactivos(
          int pagina = PaginacionParametros.PaginaPredeterminada,
             int tamPagina = PaginacionParametros.TamPaginaPredeterminado)
        {
              (pagina, tamPagina) =PaginacionParametros.Normalizar(pagina, tamPagina);

            return new ResultadoPaginado<Membresia>
            {
                Items = _repo.ObtenerInactivos(pagina, tamPagina),
                Pagina = pagina,
                TamPagina = tamPagina,
                Total = _repo.ObtenerCantidad(false)
            };
        }


        public Membresia? Obtener(int id)
        {
            if (id <= 0)
                return null;

            return _repo.ObtenerPorId(id);
        }

   
        public Membresia Crear(
    Membresia membresia,
    int idUsuarioCreador)
{
    if (membresia.IdSocio <= 0)
        throw new ReglaNegocioException(
            "Debe seleccionar un socio.");

    if (membresia.IdPlan <= 0)
        throw new ReglaNegocioException(
            "Debe seleccionar un plan.");

    var socio = _repoSocio.ObtenerPorId(membresia.IdSocio);

    if (socio == null)
        throw new ReglaNegocioException(
            "El socio seleccionado no existe.");

    if (!socio.Estado)
        throw new ReglaNegocioException(
            "No se puede crear una membresía para un socio inactivo.");

    if (_repo.ExistePorSocio(membresia.IdSocio))
        throw new ReglaNegocioException(
            "Este socio ya tiene una membresía registrada. Debe renovarla en lugar de crear otra.");

    var plan = _repoPlan.ObtenerPorId(membresia.IdPlan);

    if (plan == null)
        throw new ReglaNegocioException(
            "El plan seleccionado no existe.");

    if (!plan.Estado)
        throw new ReglaNegocioException(
            "No se puede utilizar un plan inactivo.");

    if (idUsuarioCreador <= 0)
        throw new ReglaNegocioException(
            "No se pudo identificar al usuario que registra la membresía.");

    var hoy = DateTime.Today;

    membresia.FechaInicio = hoy;

    // Todavía no tiene acceso: el período no comenzó.
    membresia.FechaFin = hoy.AddDays(-1);

    membresia.PrecioContratado = plan.Precio;
    membresia.Estado = true;
    membresia.Pagada = false;
    membresia.IdUsuarioCreador = idUsuarioCreador;

    _repo.Alta(membresia);

    return membresia;
}

 

        public Membresia? Modificar(
            int id,
            Membresia membresia)
        {
            if (id <= 0)
                return null;

            var existente = _repo.ObtenerPorId(id);

            if (existente == null)
                return null;

            if (membresia.IdSocio <= 0)
            {
                throw new ReglaNegocioException(
                    "Debe seleccionar un socio.");
            }

            if (membresia.IdPlan <= 0)
            {
                throw new ReglaNegocioException(
                    "Debe seleccionar un plan.");
            }

            // No se permite transferir una membresía a otro socio.

            if (membresia.IdSocio != existente.IdSocio)
            {
                throw new ReglaNegocioException(
                    "No se puede cambiar el socio de una membresía existente.");
            }

            var socio = _repoSocio.ObtenerPorId(
                existente.IdSocio);

            if (socio == null || !socio.Estado)
            {
                throw new ReglaNegocioException(
                    "El socio no existe o se encuentra inactivo.");
            }

            var plan = _repoPlan.ObtenerPorId(
                membresia.IdPlan);

            if (plan == null)
            {
                throw new ReglaNegocioException(
                    "El plan seleccionado no existe.");
            }

            bool cambioPlan =
                membresia.IdPlan != existente.IdPlan;

            bool periodoVencido =
                existente.FechaFin.Date < DateTime.Today;

            // El precio y el plan del período pagado se conservan.
            // Si el período está vencido, se puede preparar una renovación.

           

            if (cambioPlan && !plan.Estado)
            {
                throw new ReglaNegocioException(
                    "No se puede seleccionar un plan inactivo.");
            }

            if (cambioPlan && plan.Precio <= 0)
            {
                throw new ReglaNegocioException(
                    "El precio del plan debe ser mayor que cero.");
            }

            // Conservamos la identidad y auditoría originales.

            existente.IdMembresia = id;

            if (cambioPlan)
            {
                existente.IdPlan = plan.IdPlan;
                existente.PrecioContratado = plan.Precio;

                // El nuevo período deberá pagarse antes de habilitarse.

                existente.Pagada = false;

                // No dejamos vigente el período anterior bajo el nuevo plan.

                existente.FechaInicio = DateTime.Today;
                existente.FechaFin = DateTime.Today.AddDays(-1);
            }

            // No se modifican Estado ni IdUsuarioCreador.
            // Tampoco se aceptan fechas ni precios enviados por Vue.

            _repo.Modificacion(existente);

            return existente;
        }

        public bool DarDeBaja(int id)
        {
            if (id <= 0)
                return false;

            return _repo.Baja(id) > 0;
        }

        public bool Reactivar(int id)
        {
            if (id <= 0)
                return false;

            var membresia = _repo.ObtenerPorId(id);

            if (membresia == null)
                return false;

            // Reactivar administrativamente no habilita el ingreso.
            // La vigencia y el pago se verifican por separado.

            return _repo.Reactivar(id) > 0;
        }


        public IList<Membresia> ObtenerActivosPorSocio(
            int idSocio,
            int pagina = 1,
            int tamPagina = 10)
        {
            if (idSocio <= 0)
                return new List<Membresia>();

            if (pagina < 1)
                pagina = 1;

            if (tamPagina < 1)
                tamPagina = 10;

            return _repo.ObtenerActivosPorSocio(
                idSocio,
                pagina,
                tamPagina);
        }


         public IList<Membresia> ObtenerProximasAVencer(int dias)

        {
        if (dias < 1 || dias > 365)

        {
        throw new ReglaNegocioException(
            "La cantidad de días debe estar entre 1 y 365.");
         }

          DateTime hoy = DateTime.Today;

          DateTime fechaLimite = hoy.AddDays(dias);

         return _repo.ObtenerProximasAVencer(
           hoy,
            fechaLimite);
} 

public IList<Membresia> ObtenerVigentesEnFecha(DateTime fecha)
{
    if (fecha == DateTime.MinValue)
    {
        throw new ReglaNegocioException(
            "Debe seleccionar una fecha.");
    }

    return _repo.ObtenerVigentesEnFecha(fecha.Date);
}



    }
}
