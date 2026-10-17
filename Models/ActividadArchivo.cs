namespace Gimnasio_Alcaraz_Ballerini_Delicia.Models
{
    public class ActividadArchivo
    {
        public int IdArchivo { get; set; }

        public int IdActividad { get; set; }

        public string Ruta { get; set; } = string.Empty;

        // Para que la vista sepa si mostrar <img> o <video>
        public bool EsVideo =>
            Ruta.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
            Ruta.EndsWith(".webm", StringComparison.OrdinalIgnoreCase);
    }
}
