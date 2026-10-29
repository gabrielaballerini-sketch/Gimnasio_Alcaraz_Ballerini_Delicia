using Gimnasio_Alcaraz_Ballerini_Delicia.DAO;
using Gimnasio_Alcaraz_Ballerini_Delicia.Models;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Service
{
    public class ClaseService
    {
        private readonly IRepositorioClase _repo;
        private readonly IRepositorio<Actividad> _repoActividad;
        private readonly IRepositorioProfesor _repoProfesor;

        public ClaseService(
            IRepositorioClase repo,
            IRepositorio<Actividad> repoActividad,
            IRepositorioProfesor repoProfesor)
        {
            _repo = repo;
            _repoActividad = repoActividad;
            _repoProfesor = repoProfesor;
        }


        public ResultadoPaginado<Clase> Listar(
            bool activas,
            int? idActividad,
            int? idProfesor,
            int pagina = PaginacionParametros.PaginaPredeterminada,
             int tamPagina = PaginacionParametros.TamPaginaPredeterminado)
        {
             (pagina, tamPagina) =PaginacionParametros.Normalizar(pagina, tamPagina);

            return new ResultadoPaginado<Clase>
            {
                Items = _repo.Buscar(
                    activas,
                    idActividad,
                    idProfesor,
                    pagina,
                    tamPagina),

                Pagina = pagina,

                TamPagina = tamPagina,

                Total = _repo.Contar(
                    activas,
                    idActividad,
                    idProfesor)
            };
        }


        public IList<Clase> ObtenerGrilla()
        {
            return _repo.ObtenerGrilla();
        }


        public Clase? Obtener(int id)
        {
            return _repo.ObtenerPorId(id);
        }


        public Clase Crear(Clase clase)
        {
            Validar(clase, 0);

            clase.Estado = true;

            _repo.Alta(clase);

            return _repo.ObtenerPorId(clase.IdClase) ?? clase;
        }


        public Clase? Modificar(int id, Clase clase)
        {
            var existente = _repo.ObtenerPorId(id);

            if (existente == null)
                return null;

            clase.IdClase = id;

            Validar(clase, id);

            clase.Estado = existente.Estado;

            _repo.Modificacion(clase);

            return _repo.ObtenerPorId(id);
        }


        public bool DarDeBaja(int id)
        {
            return _repo.Baja(id) > 0;
        }


        public bool Reactivar(int id)
        {
            var clase = _repo.ObtenerPorId(id);

            if (clase == null)
                return false;

            // Se vuelve a validar: la actividad o el profesor pudieron darse de baja,
            // o el profesor tomar otra clase en ese horario.
            Validar(clase, id);

            return _repo.Reactivar(id) > 0;
        }


        private void Validar(Clase clase, int idClaseExcluir)
        {
            var actividad = _repoActividad.ObtenerPorId(clase.IdActividad);

            if (actividad == null || !actividad.Estado)
            {
                throw new ReglaNegocioException(
                    "Debe seleccionar una actividad activa.");
            }

            if (actividad.AccesoLibre)
            {
                throw new ReglaNegocioException(
                    $"\"{actividad.Nombre}\" es de acceso libre y no requiere clases programadas.");
            }

            var profesor = _repoProfesor.ObtenerPorId(clase.IdProfesor);

            if (profesor == null || !profesor.Estado)
            {
                throw new ReglaNegocioException(
                    "Debe seleccionar un profesor activo.");
            }

            if (!Enum.IsDefined(typeof(DiaSemana), clase.Dia))
            {
                throw new ReglaNegocioException(
                    "Debe seleccionar un día válido.");
            }

            var finDelDia = TimeSpan.FromHours(24);

            if (clase.HoraInicio < TimeSpan.Zero || clase.HoraInicio >= finDelDia ||
                clase.HoraFin < TimeSpan.Zero || clase.HoraFin >= finDelDia)
            {
                throw new ReglaNegocioException(
                    "Los horarios deben estar entre las 00:00 y las 23:59.");
            }

            if (clase.HoraFin <= clase.HoraInicio)
            {
                throw new ReglaNegocioException(
                    "El horario de fin debe ser posterior al horario de inicio.");
            }

            if (_repo.ExisteSuperposicionProfesor(
                clase.IdProfesor,
                clase.Dia,
                clase.HoraInicio,
                clase.HoraFin,
                idClaseExcluir))
            {
                throw new ReglaNegocioException(
                    $"El profesor {profesor.Apellido}, {profesor.Nombre} ya tiene una clase " +
                    "ese día en un horario que se superpone.");
            }
        }
    }
}
