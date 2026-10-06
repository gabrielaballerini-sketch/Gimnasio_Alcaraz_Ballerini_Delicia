using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.DAO;


namespace Gimnasio_Alcaraz_Ballerini_Delicia.Service
{
    public class SocioService
    {
        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long TamanoMaximoAvatar = 2 * 1024 * 1024; // 2 MB

        private readonly IRepositorioSocio _repo;
        private readonly string carpetaAvatars;

        public SocioService(IRepositorioSocio repo, IConfiguration config, IWebHostEnvironment env)
        {
            _repo = repo;
            carpetaAvatars = config["Almacenamiento:RutaAvatars"]
                ?? Path.Combine(env.ContentRootPath, "wwwroot", "avatars");
            Directory.CreateDirectory(carpetaAvatars);
        }

        public ResultadoPaginado<Socio> ObtenerActivos(int pagina = 1, int tamPagina = 10)
        {
            if (pagina < 1) pagina = 1;
            if (tamPagina < 1) tamPagina = 10;

            return new ResultadoPaginado<Socio>
            {
                Items = _repo.ObtenerActivos(pagina, tamPagina),
                Pagina = pagina,
                TamPagina = tamPagina,
                Total = _repo.ObtenerCantidad(true)
            };
        }

        public ResultadoPaginado<Socio> ObtenerInactivos(int pagina = 1, int tamPagina = 10)
        {
            if (pagina < 1) pagina = 1;
            if (tamPagina < 1) tamPagina = 10;

            return new ResultadoPaginado<Socio>
            {
                Items = _repo.ObtenerInactivos(pagina, tamPagina),
                Pagina = pagina,
                TamPagina = tamPagina,
                Total = _repo.ObtenerCantidad(false)
            };
        }

        public Socio? Obtener(int id)
        {
            return _repo.ObtenerPorId(id);
        }

        public Socio Crear(Socio socio)
        {
            BorrarEspacios(socio);
            Validar(socio, null);

            socio.Estado = true;
            socio.Avatar = null;

            if (socio.avatarFile != null)
                socio.Avatar = GuardarAvatar(socio.avatarFile);

            try
            {
                _repo.Alta(socio);
            }
            catch
            {
                // Si falla la base, no dejamos la imagen huérfana
                EliminarAvatar(socio.Avatar);
                throw;
            }

            socio.avatarFile = null;
            return socio;
        }

        // Devuelve null si el socio no existe
        public Socio? Modificar(int id, Socio socio)
        {
            var existente = _repo.ObtenerPorId(id);
            if (existente == null) return null;

            BorrarEspacios(socio);
            Validar(socio, id);

            socio.IdSocio = id;
            socio.Estado = existente.Estado;   // el estado solo cambia con Baja / Reactivar
            socio.Avatar = existente.Avatar;   // el cliente no puede cambiar la ruta a mano

            if (socio.avatarFile != null)
                socio.Avatar = GuardarAvatar(socio.avatarFile);

            try
            {
                _repo.Modificacion(socio);
            }
            catch
            {
                if (socio.avatarFile != null) EliminarAvatar(socio.Avatar);
                throw;
            }

            // Recién ahora que se guardó bien, borramos la foto anterior
            if (socio.avatarFile != null) EliminarAvatar(existente.Avatar);

            socio.avatarFile = null;
            return socio;
        }

        // Devuelve false si el socio no existe
        public bool DarDeBaja(int id)
        {
            return _repo.Baja(id) > 0;
        }

        // Devuelve false si el socio no existe o ya estaba activo
        public bool Reactivar(int id)
        {
            return _repo.Reactivar(id) > 0;
        }

        private static void BorrarEspacios(Socio s)
        {
            s.Nombre = s.Nombre?.Trim();
            s.Apellido = s.Apellido?.Trim();
            s.Dni = s.Dni?.Trim();
            s.Telefono = s.Telefono?.Trim();
            s.Domicilio = s.Domicilio?.Trim();
        }

        private void Validar(Socio s, int? idExistente)
        {
            if (string.IsNullOrEmpty(s.Dni))
                throw new ReglaNegocioException("El DNI es obligatorio.");

            if (_repo.ExisteDni(s.Dni, idExistente))
                throw new ReglaNegocioException("Ya existe un socio con ese DNI.");

            if (s.avatarFile != null)
            {
                var extension = Path.GetExtension(s.avatarFile.FileName).ToLowerInvariant();
                if (!ExtensionesPermitidas.Contains(extension))
                    throw new ReglaNegocioException("La imagen debe ser JPG, PNG o WEBP.");
                if (s.avatarFile.Length > TamanoMaximoAvatar)
                    throw new ReglaNegocioException("La imagen no puede superar los 2 MB.");
            }
        }

        private string GuardarAvatar(IFormFile archivo)
        {
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";

            using var stream = new FileStream(Path.Combine(carpetaAvatars, nombreArchivo), FileMode.Create);
            archivo.CopyTo(stream);

            return $"/avatars/{nombreArchivo}";
        }

        private void EliminarAvatar(string? ruta)
        {
            if (string.IsNullOrEmpty(ruta) || !ruta.StartsWith("/avatars/")) return;

            // GetFileName evita que una ruta manipulada salga de la carpeta
            var archivo = Path.Combine(carpetaAvatars, Path.GetFileName(ruta));
            if (File.Exists(archivo)) File.Delete(archivo);
        }
        public IList<Socio> Buscar(string? texto, int limite = 10)
        {
            texto = texto?.Trim();

            if (string.IsNullOrEmpty(texto) || texto.Length < 2)
                return new List<Socio>();

            if (limite < 1 || limite > 20) limite = 10;

            return _repo.Buscar(texto, limite);
        }
    }
}