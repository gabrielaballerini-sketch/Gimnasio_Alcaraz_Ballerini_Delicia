using Gimnasio_Alcaraz_Ballerini_Delicia.Models;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Service
{
    public class ActividadService
    {
        private readonly IRepositorio<Actividad> _repo;


        public ActividadService(
            IRepositorio<Actividad> repo)
        {
            _repo = repo;
        }


        public ResultadoPaginado<Actividad> ObtenerActivos(
            int pagina = 1,
            int tamPagina = 10)
        {
            if (pagina < 1)
                pagina = 1;

            if (tamPagina < 1)
                tamPagina = 10;

            return new ResultadoPaginado<Actividad>
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


        public ResultadoPaginado<Actividad> ObtenerInactivos(
            int pagina = 1,
            int tamPagina = 10)
        {
            if (pagina < 1)
                pagina = 1;

            if (tamPagina < 1)
                tamPagina = 10;

            return new ResultadoPaginado<Actividad>
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


        public Actividad? Obtener(int id)
        {
            return _repo.ObtenerPorId(id);
        }


        public Actividad Crear(
            Actividad actividad)
        {
            BorrarEspacios(actividad);

            Validar(actividad);

            actividad.Estado = true;

            _repo.Alta(actividad);

            return actividad;
        }


        public Actividad? Modificar(
            int id,
            Actividad actividad)
        {
            var existente =
                _repo.ObtenerPorId(id);

            if (existente == null)
                return null;

            BorrarEspacios(actividad);

            Validar(actividad);

            actividad.IdActividad = id;

            actividad.Estado =
                existente.Estado;

            _repo.Modificacion(actividad);

            return actividad;
        }


        public bool DarDeBaja(int id)
        {
            return _repo.Baja(id) > 0;
        }


        public bool Reactivar(int id)
        {
            return _repo.Reactivar(id) > 0;
        }


        private static void BorrarEspacios(
            Actividad actividad)
        {
            actividad.Nombre =
                actividad.Nombre?.Trim();
        }


        private void Validar(
            Actividad actividad)
        {
            if (string.IsNullOrWhiteSpace(
                actividad.Nombre))
            {
                throw new ReglaNegocioException(
                    "El nombre de la actividad es obligatorio."
                );
            }
        }
    }
}