public class Plan
{
    public int IdPlan { get; set; }
    public string Nombre { get; set; } = string.Empty;
    

    public int? UtilizacionesMensuales { get; set; } 
    public decimal Precio { get; set; }
    public bool Estado { get; set; } = true;

    // Propiedad calculada útil para la lógica
    public bool EsIlimitado => !UtilizacionesMensuales.HasValue;
}