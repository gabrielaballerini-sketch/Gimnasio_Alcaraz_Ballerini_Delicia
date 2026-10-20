using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.DAO;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Service
{
    public class UsuarioService
    {
        private readonly IRepositorioUsuario _repo;
        private readonly IRepositorioSocio _repoSocio;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public UsuarioService(
            IRepositorioUsuario repo,
            IRepositorioSocio repoSocio,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _repo = repo;
            _repoSocio = repoSocio;
            _environment = environment;
            _configuration = configuration;
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


            if (usuario.Roles == Roles.Socio)
                throw new ReglaNegocioException("Los socios se crean desde la pantalla de Socios.");

            usuario.Password = HashearPassword(usuario.Password!);
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
            if (string.IsNullOrWhiteSpace(usuario.Password) ||
            usuario.Password == existente.Password)
            {
                usuario.Password = existente.Password;
            }
            else
            {
                usuario.Password = HashearPassword(usuario.Password);
            }

            Validar(usuario, id);

            // El cambio de rol solo modifica los permisos de la cuenta.
            // No se da de baja al socio, no se reactiva y no se desvincula.
            // Así se conservan Socio ,  Membresía ,  Pago y sus IDs históricos.
            usuario.IdUsuario = existente.IdUsuario;
            usuario.Estado = existente.Estado;
            usuario.Avatar = existente.Avatar;
            usuario.NombreUsuario ??= existente.NombreUsuario;

            // IdSocio/Socio se recuperan desde el JOIN del repositorio.
            // El formulario puede enviar IdSocio = null al elegir Empleado;
            // no usamos ese valor para borrar la relación existente.
            usuario.IdSocio = existente.IdSocio;
            usuario.Socio = existente.Socio;

            // Para asignar el rol Socio, debe existir una ficha de socio.
            // Si nunca tuvo una, usar la acción "Convertir en socio".
            if (usuario.Roles == Roles.Socio && !existente.IdSocio.HasValue)
            {
                throw new ReglaNegocioException(
                    "Este usuario todavía no tiene ficha de socio. Utilizá el botón 'Convertir en socio'.");
            }

            // Guardamos únicamente usuario/rol. No tocar la tabla socio.


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
            if (!await FirmasArchivo.CoincideConExtensionAsync(archivo, extension))
            {
                throw new ReglaNegocioException(
                    "El contenido del archivo no coincide con su formato.");
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

            var nombreUsuario =
                string.IsNullOrWhiteSpace(nombreUsuarioNuevo)
                    ? null
                    : nombreUsuarioNuevo.Trim();

            if (string.IsNullOrWhiteSpace(email))
                throw new ReglaNegocioException(
                    "El email es obligatorio.");

            if (_repo.ExisteEmail(email, id))
                throw new ReglaNegocioException(
                    "Ya existe un usuario con ese email.");

            if (nombreUsuario != null && nombreUsuario.Length > 50)
                throw new ReglaNegocioException(
                    "El nombre de usuario no puede superar los 50 caracteres.");

            if (!string.IsNullOrWhiteSpace(nuevaPassword))
            {
                if (string.IsNullOrWhiteSpace(passwordActual) ||
                    !VerificarPassword(existente, passwordActual))
                {
                    throw new ReglaNegocioException(
                        "La contraseña actual no es correcta.");
                }

                if (nuevaPassword != confirmarPassword)
                    throw new ReglaNegocioException(
                        "La confirmación de la contraseña no coincide.");

                if (!System.Text.RegularExpressions.Regex.IsMatch(
                    nuevaPassword,
                    @"^[a-zA-ZñÑ\d\s]{6,20}$"))
                {
                    throw new ReglaNegocioException(
                        "La contraseña debe tener entre 6 y 20 caracteres (letras y números).");
                }

                existente.Password = HashearPassword(nuevaPassword);
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


        private string HashearPassword(string password)
        {
            var salt = _configuration["Salt"];

            if (string.IsNullOrWhiteSpace(salt))
                throw new InvalidOperationException(
                    "Falta configurar Salt en appsettings.json.");

            return Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password: password,
                    salt: Encoding.ASCII.GetBytes(salt),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 1000,
                    numBytesRequested: 256 / 8
                )
            );
        }

        public bool VerificarPassword(Usuario usuario, string passwordIngresada)
        {
            if (string.IsNullOrWhiteSpace(passwordIngresada) ||
                string.IsNullOrEmpty(usuario.Password))
            {
                return false;
            }

            // Compatibilidad con usuarios antiguos en texto plano.
            if (string.Equals(
                usuario.Password,
                passwordIngresada,
                StringComparison.Ordinal))
            {
                usuario.Password = HashearPassword(passwordIngresada);

                _repo.Modificacion(usuario);

                return true;
            }

            // Verificar usuarios que ya tienen hash.
            var hashIngresado = HashearPassword(passwordIngresada);

            return string.Equals(
                usuario.Password,
                hashIngresado,
                StringComparison.Ordinal);
        }
        // Valida y arma la cuenta de un socio nuevo: rol Socio, activa y con la contraseña hasheada
        public Usuario PrepararCuentaSocio(string email, string password)
        {
            var usuario = new Usuario
            {
                Email = email?.Trim(),
                Password = password,
                Roles = Roles.Socio,
                Estado = true
            };

            Validar(usuario, null);   // email obligatorio y único, contraseña obligatoria

            usuario.Password = HashearPassword(usuario.Password!);

            return usuario;
        }

        // Solo el rol Socio se vincula con una ficha, y cada socio tiene como máximo un usuario


        public Usuario? ConvertirASocio(int idUsuario, Socio datos)
        {
            var usuario = _repo.ObtenerPorId(idUsuario);

            if (usuario == null)
                return null;

            if (!usuario.Estado)
                throw new ReglaNegocioException(
                    "No se puede convertir un usuario inactivo.");

            if (usuario.Roles == Roles.Socio)
                throw new ReglaNegocioException(
                    "El usuario ya tiene el rol de socio.");

            datos ??= new Socio();

            datos.Nombre = datos.Nombre?.Trim();
            datos.Apellido = datos.Apellido?.Trim();
            datos.Dni = datos.Dni?.Trim();
            datos.Telefono = datos.Telefono?.Trim();
            datos.Domicilio = datos.Domicilio?.Trim();

            // Si tiene una ficha histórica vinculada, se conservan sus datos.
            var fichaAnterior = _repoSocio.ObtenerPorIdUsuario(idUsuario);

            // Solo si nunca tuvo ficha se validan los datos (mismas reglas que el alta de socio).
            if (fichaAnterior == null)
            {
                var resultados = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

                System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
                    datos,
                    new System.ComponentModel.DataAnnotations.ValidationContext(datos),
                    resultados,
                    true);

                if (resultados.Count > 0)
                {
                    throw new ReglaNegocioException(
                        "Revisá los datos del socio.",
                        resultados
                            .Select(r => r.ErrorMessage ?? "Dato inválido")
                            .Distinct()
                            .ToList());
                }
            }

            try
            {
                _repoSocio.ConvertirUsuarioEnSocio(idUsuario, datos);
            }
            catch (InvalidOperationException ex)
            {
                throw new ReglaNegocioException(ex.Message);
            }

            return _repo.ObtenerPorId(idUsuario);




        }
    }
}