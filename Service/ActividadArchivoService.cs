using Gimnasio_Alcaraz_Ballerini_Delicia.DAO;
using Gimnasio_Alcaraz_Ballerini_Delicia.Models;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Service
{
    public class ActividadArchivoService
    {
        private readonly IRepositorio<Actividad> _repoActividad;
        private readonly IRepositorioActividadArchivo _repo;
        private readonly IWebHostEnvironment _environment;

        // Carpeta pública donde se guardan los archivos (dentro de wwwroot)
        private const string RutaPublica = "/imagenes/actividades/";

        private const int MaxArchivosPorActividad = 10;
        private const long MaxImagen = 5 * 1024 * 1024;
        private const long MaxVideo = 20 * 1024 * 1024;

        // extensión permitida -> tipo MIME esperado
        private static readonly Dictionary<string, string> Formatos = new()
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp",
            [".gif"] = "image/gif",
            [".mp4"] = "video/mp4",
            [".webm"] = "video/webm"
        };


        public ActividadArchivoService(
            IRepositorio<Actividad> repoActividad,
            IRepositorioActividadArchivo repo,
            IWebHostEnvironment environment)
        {
            _repoActividad = repoActividad;
            _repo = repo;
            _environment = environment;
        }


        // Devuelve null si la actividad no existe
        public IList<ActividadArchivo>? Listar(int idActividad)
        {
            if (_repoActividad.ObtenerPorId(idActividad) == null)
                return null;

            return _repo.ObtenerPorActividad(idActividad);
        }


        // Devuelve null si la actividad no existe
        public async Task<ActividadArchivo?> Subir( int idActividad,IFormFile archivo)
        {
            if (_repoActividad.ObtenerPorId(idActividad) == null)
                return null;

            if (archivo == null || archivo.Length == 0)
            {
                throw new ReglaNegocioException(
                    "Debe seleccionar una imagen o un video.");
            }

            var extension = Path.GetExtension(archivo.FileName)
                .ToLowerInvariant();

            if (!Formatos.TryGetValue(extension, out var tipoEsperado) ||
                !string.Equals(
                    archivo.ContentType,
                    tipoEsperado,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ReglaNegocioException(
                    "Formato no permitido. Usá JPG, PNG, WEBP, GIF, MP4 o WEBM.");
            }

            var esVideo = extension == ".mp4" || extension == ".webm";

            if (archivo.Length > (esVideo ? MaxVideo : MaxImagen))
            {
                throw new ReglaNegocioException(
                    esVideo
                        ? "El video no puede superar los 20 MB."
                        : "La imagen no puede superar los 5 MB.");
            }
            if (!await FirmasArchivo.CoincideConExtensionAsync(archivo, extension))
            {
                throw new ReglaNegocioException(
                    "El contenido del archivo no coincide con su formato.");
            }


            if (_repo.Contar(idActividad) >= MaxArchivosPorActividad)
            {
                throw new ReglaNegocioException(
                    $"Una actividad puede tener hasta {MaxArchivosPorActividad} archivos. " +
                    "Eliminá alguno para subir otro.");
            }

            var carpeta = ObtenerCarpeta();
            Directory.CreateDirectory(carpeta);

            // Nombre generado: nunca se usa el nombre original del archivo.
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var rutaFisica = Path.Combine(carpeta, nombreArchivo);

            var nuevo = new ActividadArchivo
            {
                IdActividad = idActividad,
                Ruta = RutaPublica + nombreArchivo
            };

            try
            {
                using (var stream = new FileStream(
                    rutaFisica,
                    FileMode.CreateNew))
                {
                    await archivo.CopyToAsync(stream);
                }

                _repo.Alta(nuevo);
            }
            catch
            {
                // Si algo falla, no queda un archivo suelto en el disco.
                if (File.Exists(rutaFisica))
                    File.Delete(rutaFisica);

                throw;
            }

            return nuevo;
        }


        public bool Eliminar(int idArchivo)
        {
            var archivo = _repo.ObtenerPorId(idArchivo);

            if (archivo == null)
                return false;

            _repo.Eliminar(idArchivo);
            EliminarArchivoFisico(archivo.Ruta);

            return true;
        }


        private string ObtenerCarpeta()
        {
            return Path.Combine(
                _environment.WebRootPath
                    ?? Path.Combine(_environment.ContentRootPath, "wwwroot"),
                "imagenes",
                "actividades");
        }


        private void EliminarArchivoFisico(string? ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta) ||
                !ruta.StartsWith(
                    RutaPublica,
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Path.GetFileName evita que una ruta rara escape de la carpeta.
            var rutaFisica = Path.Combine(
                ObtenerCarpeta(),
                Path.GetFileName(ruta));

            if (File.Exists(rutaFisica))
                File.Delete(rutaFisica);
        }
    }
}
