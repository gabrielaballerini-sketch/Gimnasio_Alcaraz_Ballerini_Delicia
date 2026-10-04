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
            int pagina = 1,
            int tamPagina = 10)
        {
            if (pagina < 1)
                pagina = 1;

            if (tamPagina < 1)
                tamPagina = 10;

            return new ResultadoPaginado<Membresia>
            {
                Items = _repo.ObtenerActivos(
                    pagina,
                    tamPagina
                ),

                Pagina = pagina,

                TamPagina = tamPagina,

                Total = _repo.ObtenerCantidad(true)
            };
        }

        public ResultadoPaginado<Membresia> ObtenerInactivos(
            int pagina = 1,
            int tamPagina = 10)
        {
            if (pagina < 1)
                pagina = 1;

            if (tamPagina < 1)
                tamPagina = 10;

            return new ResultadoPaginado<Membresia>
            {
                Items = _repo.ObtenerInactivos(
                    pagina,
                    tamPagina
                ),

                Pagina = pagina,

                TamPagina = tamPagina,

                Total = _repo.ObtenerCantidad(false)
            };
        }

        public Membresia? Obtener(int id)
        {
            return _repo.ObtenerPorId(id);
        }

        public Membresia Crear(
            Membresia membresia)
        {
            if (membresia.IdSocio <= 0)
            {
                throw new ReglaNegocioException(
                    "Debe seleccionar un socio."
                );
            }

            if (membresia.IdPlan <= 0)
            {
                throw new ReglaNegocioException(
                    "Debe seleccionar un plan."
                );
            }

            var socio =
                _repoSocio.ObtenerPorId(
                    membresia.IdSocio
                );

            if (socio == null)
            {
                throw new ReglaNegocioException(
                    "El socio seleccionado no existe."
                );
            }

            if (!socio.Estado)
            {
                throw new ReglaNegocioException(
                    "No se puede crear una membresía para un socio inactivo."
                );
            }

            var plan =
                _repoPlan.ObtenerPorId(
                    membresia.IdPlan
                );

            if (plan == null)
            {
                throw new ReglaNegocioException(
                    "El plan seleccionado no existe."
                );
            }

            if (!plan.Estado)
            {
                throw new ReglaNegocioException(
                    "No se puede utilizar un plan inactivo."
                );
            }

            if (membresia.FechaInicio.Date < DateTime.Today)
            {
                throw new ReglaNegocioException(
                    "La fecha de inicio no puede ser anterior a la fecha actual."
                );
            }

            membresia.FechaInicio =
                membresia.FechaInicio.Date;

            membresia.FechaFin =
                membresia.FechaInicio.AddMonths(1);

            membresia.PrecioContratado =
                plan.Precio;

            membresia.Estado = true;

            membresia.IdUsuarioCreador = 1;

            _repo.Alta(membresia);

            return membresia;
        }

        public Membresia? Modificar(
            int id,
            Membresia membresia)
        {
            var existente =
                _repo.ObtenerPorId(id);

            if (existente == null)
                return null;

            if (membresia.IdSocio <= 0)
            {
                throw new ReglaNegocioException(
                    "Debe seleccionar un socio."
                );
            }

            if (membresia.IdPlan <= 0)
            {
                throw new ReglaNegocioException(
                    "Debe seleccionar un plan."
                );
            }

            var socio =
                _repoSocio.ObtenerPorId(
                    membresia.IdSocio
                );

            if (socio == null)
            {
                throw new ReglaNegocioException(
                    "El socio seleccionado no existe."
                );
            }

            if (!socio.Estado)
            {
                throw new ReglaNegocioException(
                    "No se puede asignar una membresía a un socio inactivo."
                );
            }

            var plan =
                _repoPlan.ObtenerPorId(
                    membresia.IdPlan
                );

            if (plan == null)
            {
                throw new ReglaNegocioException(
                    "El plan seleccionado no existe."
                );
            }

            if (!plan.Estado)
            {
                throw new ReglaNegocioException(
                    "No se puede utilizar un plan inactivo."
                );
            }

            if (membresia.FechaInicio.Date < DateTime.Today)
            {
                throw new ReglaNegocioException(
                    "La fecha de inicio no puede ser anterior a la fecha actual."
                );
            }

            membresia.IdMembresia = id;

            membresia.FechaInicio =
                membresia.FechaInicio.Date;

            membresia.FechaFin =
                membresia.FechaInicio.AddMonths(1);

            membresia.PrecioContratado =
                plan.Precio;

            membresia.Estado =
                existente.Estado;

            membresia.IdUsuarioCreador =
                existente.IdUsuarioCreador;

            _repo.Modificacion(membresia);

            return membresia;
        }

        public bool DarDeBaja(int id)
        {
            return _repo.Baja(id) > 0;
        }

        public bool Reactivar(int id)
        {
            return _repo.Reactivar(id) > 0;
        }

        public IList<Membresia> ObtenerActivosPorSocio(
            int idSocio,
            int pagina = 1,
            int tamPagina = 10)
        {
            if (idSocio <= 0)
                return new List<Membresia>();

            return _repo.ObtenerActivosPorSocio(
                idSocio,
                pagina,
                tamPagina
            );
        }
    }
}