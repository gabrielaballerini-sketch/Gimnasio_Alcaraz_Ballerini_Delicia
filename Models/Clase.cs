using System.ComponentModel.DataAnnotations;
namespace Gimnasio_Alcaraz_Ballerini_Delicia.Models{
public class Clase
{
    public int IdClase { get; set; }
    
  
    public int IdActividad { get; set; }
    public Actividad Actividad { get; set; } = null!;
    
    // Quién la dicta
    public int IdProfesor { get; set; }
    public Profesor Profesor { get; set; } = null!;
    
    // Cuándo se dicta
    [Required(ErrorMessage ="Debe seleccionar un dia")]
    
    public DiaSemana Dia { get; set; }      // Ej: Lunes
    [Required(ErrorMessage ="Debe seleccionar un Horario de inicio")]
    public TimeSpan HoraInicio { get; set; } // Ej: 18:00
    [Required(ErrorMessage ="Debe seleccionar un Horario de fin")]
    public TimeSpan HoraFin { get; set; }    // Ej: 19:00
    
    public bool Estado { get; set; } = true;
}

public enum DiaSemana
{
    Lunes=1,
    Martes=2,
    Miercoles=3,
    Jueves=4,
    Viernes=5,
    Sabado=6,
    Domingo=7
}}