public class Membresia
{
    public int IdMembresia { get; set; }
    
    public int SocioId { get; set; }
    public Socio Socio { get; set; } = null!;
    
    public int PlanId { get; set; }
    public Plan Plan { get; set; } = null!;
    
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal PrecioContratado { get; set; } 
    public bool Estado { get; set; } = true;

    // Auditoría
    public int CreadoPor { get; set; } 
}