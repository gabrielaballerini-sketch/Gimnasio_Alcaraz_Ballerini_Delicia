using System.ComponentModel.DataAnnotations;
namespace Gimnasio_Alcaraz_Ballerini_Delicia.Models{
public class Plan
{
    public int IdPlan { get; set; }
    [Required(ErrorMessage ="Debe seleccionar un plan ")]
   
    public string? Nombre { get; set; } 
    

    public int? UtilizacionesMensuales { get; set; } 
    [Required(ErrorMessage ="Debe seleccionar un precio ")]
    
    public decimal Precio { get; set; }
    public bool Estado { get; set; } = true;

    // Propiedad calculada útil para la lógica
    public bool EsIlimitado => !UtilizacionesMensuales.HasValue;
}}