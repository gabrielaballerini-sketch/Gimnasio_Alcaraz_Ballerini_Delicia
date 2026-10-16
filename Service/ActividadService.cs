using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.DAO;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Service
{
    public class ActividadService
    {
        private readonly IRepositorio<Actividad> _repo;
        private readonly IWebHostEnvironment _environment;

        // Carpeta pública donde se guardan los archivos (dentro de wwwroot)
        private const string RutaPublica = "/imagenes/actividades/";

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


        public ActividadService(
            IRepositorio<Actividad> repo,
            IWebHostEnvironment environment)
        {
            _repo = repo;
            _environment = environment;
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

            // El archivo no se cambia acá: se maneja con SubirArchivo / QuitarArchivo.
            actividad.Archivo =
                existente.Archivo;

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


        // SUBIR O REEMPLAZAR LA IMAGEN / VIDEO DE UNA ACTIVIDAD
        public async Task<Actividad?> SubirArchivo(
            int id,
            IFormFile archivo)
        {
            var actividad = _repo.ObtenerPorId(id);

            if (actividad == null)
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

            var carpeta = ObtenerCarpeta();
            Directory.CreateDirectory(carpeta);

            // Nombre generado: nunca se usa el nombre original del archivo.
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var rutaFisica = Path.Combine(carpeta, nombreArchivo);
            var rutaAnterior = actividad.Archivo;

            try
            {
                using (var stream = new FileStream(
                    rutaFisica,
                    FileMode.CreateNew))
                {
                    await archivo.CopyToAsync(stream);
                }

                actividad.Archivo = RutaPublica + nombreArchivo;

                _repo.Modificacion(actividad);
            }
            catch
            {
                if (File.Exists(rutaFisica))
                    File.Delete(rutaFisica);

                throw;
            }

            // Recién ahora, con todo guardado, se borra el archivo viejo.
            EliminarArchivoFisico(rutaAnterior);

            return actividad;
        }


        // QUITAR LA IMAGEN / VIDEO DE UNA ACTIVIDAD
        public Actividad? QuitarArchivo(int id)
        {
            var actividad = _repo.ObtenerPorId(id);

            if (actividad == null)
                return null;

            var rutaAnterior = actividad.Archivo;

            actividad.Archivo = null;
            _repo.Modificacion(actividad);

            EliminarArchivoFisico(rutaAnterior);

            return actividad;
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