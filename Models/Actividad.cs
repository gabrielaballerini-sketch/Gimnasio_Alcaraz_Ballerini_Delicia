using System.ComponentModel.DataAnnotations;
namespace Gimnasio_Alcaraz_Ballerini_Delicia.Models{

public class Actividad
{
    public int IdActividad { get; set; }
    [Required(ErrorMessage ="Debe ingresar un nombre")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+(?:\s+[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+)*$",
        ErrorMessage = "El nombre solo puede llevar letras")]
    public string Nombre { get; set; } = string.Empty;
    
    public bool Estado { get; set; } = true;
}
}