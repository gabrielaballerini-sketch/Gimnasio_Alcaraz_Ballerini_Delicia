namespace Gimnasio_Alcaraz_Ballerini_Delicia.Models
{
    public class EstadoAccesoAsistencia
    {
        public int IdSocio { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Dni { get; set; } = "";

        public bool PuedeIngresar { get; set; }
        public string? Motivo { get; set; }

        public int? IdMembresia { get; set; }
        public string? Plan { get; set; }
        public DateTime? VenceEl { get; set; }

        public bool Ilimitado { get; set; }
        public int? Limite { get; set; }
        public int Usadas { get; set; }


        // para q el socio pueda ver el periodo vigente de mi membresia
        public DateTime? VigenteDesde { get; set; }  

    }
}