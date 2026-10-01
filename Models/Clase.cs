public class Clase
{
    public int IdClase { get; set; }
    
  
    public int ActividadId { get; set; }
    public Actividad Actividad { get; set; } = null!;
    
    // Quién la dicta
    public int ProfesorId { get; set; }
    public Profesor Profesor { get; set; } = null!;
    
    // Cuándo se dicta
    public DiaSemana Dia { get; set; }      // Ej: Lunes
    public TimeSpan HoraInicio { get; set; } // Ej: 18:00
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
}