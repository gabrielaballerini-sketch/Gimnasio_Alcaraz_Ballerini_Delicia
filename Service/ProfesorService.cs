using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.DAO;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Service
{
    public class ProfesorService
    {
        private readonly IRepositorioProfesor _repo;

        public ProfesorService(IRepositorioProfesor repo)
        {
            _repo = repo;
        }

        public ResultadoPaginado<Profesor> ObtenerActivos(
            int pagina = 1,
            int tamPagina = 10)
        {
            if (pagina < 1)
                pagina = 1;

            if (tamPagina < 1)
                tamPagina = 10;

            return new ResultadoPaginado<Profesor>
            {
                Items = _repo.ObtenerActivos(pagina, tamPagina),
                Pagina = pagina,
                TamPagina = tamPagina,
                Total = _repo.ObtenerCantidad(true)
            };
        }

        public ResultadoPaginado<Profesor> ObtenerInactivos(
            int pagina = 1,
            int tamPagina = 10)
        {
            if (pagina < 1)
                pagina = 1;

            if (tamPagina < 1)
                tamPagina = 10;

            return new ResultadoPaginado<Profesor>
            {
                Items = _repo.ObtenerInactivos(pagina, tamPagina),
                Pagina = pagina,
                TamPagina = tamPagina,
                Total = _repo.ObtenerCantidad(false)
            };
        }

        public Profesor? Obtener(int id)
        {
            return _repo.ObtenerPorId(id);
        }

        public Profesor Crear(Profesor profesor)
        {
            BorrarEspacios(profesor);

            Validar(profesor, null);

            profesor.Estado = true;

            _repo.Alta(profesor);

            return profesor;
        }

        public Profesor? Modificar(int id, Profesor profesor)
        {
            var existente = _repo.ObtenerPorId(id);

            if (existente == null)
                return null;

            BorrarEspacios(profesor);

            Validar(profesor, id);

            profesor.IdProfesor = id;

            // El estado se modifica únicamente mediante
            // DarDeBaja / Reactivar.
            profesor.Estado = existente.Estado;

            _repo.Modificacion(profesor);

            return profesor;
        }

        public bool DarDeBaja(int id)
        {
            return _repo.Baja(id) > 0;
        }

        public bool Reactivar(int id)
        {
            return _repo.Reactivar(id) > 0;
        }

        public Profesor? ObtenerPorDni(string dni)
        {
            dni = dni?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(dni))
                return null;

            return _repo.ObtenerPorDni(dni);
        }

        private static void BorrarEspacios(Profesor profesor)
        {
            profesor.Nombre = profesor.Nombre?.Trim();
            profesor.Apellido = profesor.Apellido?.Trim();
            profesor.Dni = profesor.Dni?.Trim();
            profesor.Telefono = profesor.Telefono?.Trim();
            profesor.Domicilio = profesor.Domicilio?.Trim();
        }

        private void Validar(
            Profesor profesor,
            int? idExistente)
        {
            if (string.IsNullOrWhiteSpace(profesor.Nombre))
                throw new ReglaNegocioException(
                    "El nombre del profesor es obligatorio.");

            if (string.IsNullOrWhiteSpace(profesor.Apellido))
                throw new ReglaNegocioException(
                    "El apellido del profesor es obligatorio.");

            if (string.IsNullOrWhiteSpace(profesor.Dni))
                throw new ReglaNegocioException(
                    "El DNI del profesor es obligatorio.");

            if (string.IsNullOrWhiteSpace(profesor.Telefono))
                throw new ReglaNegocioException(
                    "El teléfono del profesor es obligatorio.");

            if (string.IsNullOrWhiteSpace(profesor.Domicilio))
                throw new ReglaNegocioException(
                    "El domicilio del profesor es obligatorio.");

            if (_repo.ExisteDni(profesor.Dni, idExistente))
                throw new ReglaNegocioException(
                    "Ya existe un profesor con ese DNI.");
        }
    }
}