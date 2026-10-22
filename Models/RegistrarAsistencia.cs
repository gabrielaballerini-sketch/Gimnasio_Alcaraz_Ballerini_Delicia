using System.ComponentModel.DataAnnotations;
namespace Gimnasio_Alcaraz_Ballerini_Delicia.Models
{
      public class RegistrarAsistencia
    {
        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [RegularExpression( @"^\d{7,10}$")]
        public string Dni { get; set; } = "";

        [Required(ErrorMessage = "El tipo de asistencia es obligatorio.")]
        public TipoAsistencia Tipo { get; set; }

        public int? IdClase { get; set; }
    }
}