public class Asistencia
{
    public int IdAsistencia { get; set; }
    
    public int SocioId { get; set; }
    public Socio Socio { get; set; } = null!;
    
    // Nullable: si es Musculación, ClaseId es null
    public int? ClaseId { get; set; }
    public Clase? Clase { get; set; }
    
  
    public DateTime FechaHoraIngreso { get; set; }
}