using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.DAO;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Service
{
    public class UsuarioService
    {
        private readonly IRepositorioUsuario _repo;
        private readonly IWebHostEnvironment _environment;

        public UsuarioService(
            IRepositorioUsuario repo,
            IWebHostEnvironment environment)
        {
            _repo = repo;
            _environment = environment;
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
                Items = _repo.ObtenerActivos(pagina, tamPagina),
                Pagina = pagina,
                TamPagina = tamPagina,
                Total = _repo.ObtenerCantidad(true)
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
                Items = _repo.ObtenerInactivos(pagina, tamPagina),
                Pagina = pagina,
                TamPagina = tamPagina,
                Total = _repo.ObtenerCantidad(false)
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
            usuario.Email = usuario.Email?.Trim();

            Validar(usuario, null);

            usuario.Estado = true;

          
            usuario.Avatar = null;

            _repo.Alta(usuario);

            return usuario;
        }

        public Usuario? Modificar(int id, Usuario usuario)
        {
            var existente = _repo.ObtenerPorId(id);

            if (existente == null)
                return null;

            usuario.Email = usuario.Email?.Trim();

            Validar(usuario, id);

            usuario.IdUsuario = id;
            usuario.Estado = existente.Estado;

           
            usuario.Avatar = existente.Avatar;

            
            usuario.NombreUsuario ??= existente.NombreUsuario;


            _repo.Modificacion(usuario);

            return usuario;
        }

        public async Task<Usuario?> ActualizarAvatar(
            int id,
            IFormFile archivo)
        {
            var usuario = _repo.ObtenerPorId(id);

            if (usuario == null)
                return null;

            if (archivo == null || archivo.Length == 0)
            {
                throw new ReglaNegocioException(
                    "Debe seleccionar una imagen."
                );
            }

  
            if (archivo.Length > 5 * 1024 * 1024)
            {
                throw new ReglaNegocioException(
                    "La imagen no puede superar los 5 MB."
                );
            }

            var extensionesPermitidas = new[]
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            var extension = Path.GetExtension(
                archivo.FileName
            ).ToLowerInvariant();

            if (!extensionesPermitidas.Contains(extension))
            {
                throw new ReglaNegocioException(
                    "La imagen debe ser JPG, JPEG, PNG o WEBP."
                );
            }

            var tiposPermitidos = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            if (!tiposPermitidos.Contains(
                archivo.ContentType.ToLowerInvariant()))
            {
                throw new ReglaNegocioException(
                    "El formato de imagen no es válido."
                );
            }

            var carpeta = Path.Combine(
                _environment.WebRootPath
                    ?? Path.Combine(_environment.ContentRootPath, "wwwroot"),
                "imagenes",
                "avatares"
            );

            Directory.CreateDirectory(carpeta);

           
            var nombreArchivo =
                $"{Guid.NewGuid()}{extension}";

            var rutaFisica = Path.Combine(
                carpeta,
                nombreArchivo
            );

            var rutaAnterior = usuario.Avatar;

            try
            {
                using (var stream = new FileStream(
                    rutaFisica,
                    FileMode.CreateNew))
                {
                    await archivo.CopyToAsync(stream);
                }

                usuario.Avatar =
                    $"/imagenes/avatares/{nombreArchivo}";

                _repo.Modificacion(usuario);
            }
            catch
            {
                if (File.Exists(rutaFisica))
                    File.Delete(rutaFisica);

                throw;
            }

           
            EliminarArchivoAvatarAnterior(rutaAnterior);

            return usuario;
        }


        public Usuario? ActualizarPerfil(
            int id,
            string? emailNuevo,
            string? nombreUsuarioNuevo,
            string? passwordActual,
            string? nuevaPassword,
            string? confirmarPassword)
        {
            var existente = _repo.ObtenerPorId(id);

            if (existente == null)
                return null;

            var email = emailNuevo?.Trim();
            var nombreUsuario = string.IsNullOrWhiteSpace(nombreUsuarioNuevo)
                ? null
                : nombreUsuarioNuevo.Trim();

            if (string.IsNullOrWhiteSpace(email))
                throw new ReglaNegocioException("El email es obligatorio.");

            if (_repo.ExisteEmail(email, id))
                throw new ReglaNegocioException("Ya existe un usuario con ese email.");

            if (nombreUsuario != null && nombreUsuario.Length > 50)
                throw new ReglaNegocioException("El nombre de usuario no puede superar los 50 caracteres.");


            if (!string.IsNullOrWhiteSpace(nuevaPassword))
            {
                if (passwordActual != existente.Password)
                    throw new ReglaNegocioException("La contraseña actual no es correcta.");

                if (nuevaPassword != confirmarPassword)
                    throw new ReglaNegocioException("La confirmación de la contraseña no coincide.");

                if (!System.Text.RegularExpressions.Regex.IsMatch(
                        nuevaPassword, @"^[a-zA-ZñÑ\d\s]{6,20}$"))
                    throw new ReglaNegocioException(
                        "La contraseña debe tener entre 6 y 20 caracteres (letras y números).");

                existente.Password = nuevaPassword;
            }

    
            existente.Email = email;
            existente.NombreUsuario = nombreUsuario;

            _repo.Modificacion(existente);

            return existente;
        }
   
        public bool DarDeBaja(int id)
        {
            return _repo.Baja(id) > 0;
        }

        public bool Reactivar(int id)
        {
            return _repo.Reactivar(id) > 0;
        }

        private void EliminarArchivoAvatarAnterior(string? ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta))
                return;

            const string prefijo = "/imagenes/avatares/";

           
            if (!ruta.StartsWith(
                prefijo,
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var nombre = Path.GetFileName(ruta);

            var carpeta = Path.Combine(
                _environment.WebRootPath
                    ?? Path.Combine(_environment.ContentRootPath, "wwwroot"),
                "imagenes",
                "avatares"
            );

            var rutaFisica = Path.Combine(carpeta, nombre);

            if (File.Exists(rutaFisica))
                File.Delete(rutaFisica);
        }

        private void Validar(
            Usuario usuario,
            int? idExistente)
        {
            if (string.IsNullOrWhiteSpace(usuario.Email))
            {
                throw new ReglaNegocioException(
                    "El email es obligatorio."
                );
            }

            if (_repo.ExisteEmail(usuario.Email, idExistente))
            {
                throw new ReglaNegocioException(
                    "Ya existe un usuario con ese email."
                );
            }

            if (string.IsNullOrWhiteSpace(usuario.Password))
            {
                throw new ReglaNegocioException(
                    "El password es obligatorio."
                );
            }
        }
    }
}