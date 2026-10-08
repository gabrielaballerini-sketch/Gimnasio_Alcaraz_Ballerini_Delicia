using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.DAO;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Service
{
    public class UsuarioService
    {
        private readonly IRepositorioUsuario _repo;

        public UsuarioService(IRepositorioUsuario repo)
        {
            _repo = repo;
        }

        public ResultadoPaginado<Usuario> ObtenerActivos(
            int pagina = 1,
            int tamPagina = 10)
        {
            if (pagina < 1)
                pagina = 1;

            if (tamPagina < 1)
                tamPagina = 10;

            return new ResultadoPaginado<Usuario>
            {
                Items =
                    _repo.ObtenerActivos(
                        pagina,
                        tamPagina
                    ),

                Pagina = pagina,

                TamPagina = tamPagina,

                Total =
                    _repo.ObtenerCantidad(true)
            };
        }

        public ResultadoPaginado<Usuario> ObtenerInactivos(
            int pagina = 1,
            int tamPagina = 10)
        {
            if (pagina < 1)
                pagina = 1;

            if (tamPagina < 1)
                tamPagina = 10;

            return new ResultadoPaginado<Usuario>
            {
                Items =
                    _repo.ObtenerInactivos(
                        pagina,
                        tamPagina
                    ),

                Pagina = pagina,

                TamPagina = tamPagina,

                Total =
                    _repo.ObtenerCantidad(false)
            };
        }

        public Usuario? Obtener(int id)
        {
            return _repo.ObtenerPorId(id);
        }

        public Usuario? ObtenerPorEmail(string email)
         
         {
         if (string.IsNullOrWhiteSpace(email))
        return null;

           return _repo.ObtenerPorEmail(email.Trim());
           }



        public Usuario Crear(Usuario usuario)
        {
            usuario.Email =
                usuario.Email?.Trim();

            Validar(usuario, null);

            usuario.Estado = true;

            _repo.Alta(usuario);

            return usuario;
        }

        public Usuario? Modificar(
            int id,
            Usuario usuario)
        {
            var existente =
                _repo.ObtenerPorId(id);

            if (existente == null)
                return null;

            usuario.Email =
                usuario.Email?.Trim();

            Validar(usuario, id);

            usuario.IdUsuario = id;

            usuario.Estado =
                existente.Estado;

            _repo.Modificacion(usuario);

            return usuario;
        }

        public bool DarDeBaja(int id)
        {
            return _repo.Baja(id) > 0;
        }

        public bool Reactivar(int id)
        {
            return _repo.Reactivar(id) > 0;
        }

        private void Validar(
            Usuario usuario,
            int? idExistente)
        {
            if (string.IsNullOrWhiteSpace(
                usuario.Email))
            {
                throw new ReglaNegocioException(
                    "El email es obligatorio."
                );
            }

            if (_repo.ExisteEmail(
                usuario.Email,
                idExistente))
            {
                throw new ReglaNegocioException(
                    "Ya existe un usuario con ese email."
                );
            }

            if (string.IsNullOrWhiteSpace(
                usuario.Password))
            {
                throw new ReglaNegocioException(
                    "El password es obligatorio."
                );
            }
        }
    }
}