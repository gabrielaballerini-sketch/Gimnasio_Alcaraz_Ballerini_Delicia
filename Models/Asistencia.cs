namespace Gimnasio_Alcaraz_Ballerini_Delicia.Models
{
    public class Asistencia
    {
        public int IdAsistencia { get; set; }

        // Auditoría: el socio es quien registra su propia asistencia
        public int IdSocio { get; set; }
        public Socio Socio { get; set; } = new Socio();

        public TipoAsistencia Tipo { get; set; }

        // Nulo cuando Tipo = Musculacion
        public int? IdClase { get; set; }
        public Clase? Clase { get; set; }

        public DateTime FechaHoraIngreso { get; set; }
    }

    public enum TipoAsistencia
    {
        Clase = 1,
        Musculacion = 2
    }
}