using System.ComponentModel.DataAnnotations;
namespace Gimnasio_Alcaraz_Ballerini_Delicia.Models{
public class Membresia
{
    public int IdMembresia { get; set; }
    
    public int IdSocio { get; set; }
    public Socio Socio { get; set; } = null!;
    
    public int IdPlan { get; set; }
    public Plan Plan { get; set; } = null!;
    
    [Required(ErrorMessage ="Debe seleccionar una fecha de inicio")]
    
    public DateTime FechaInicio { get; set; }
   
    public DateTime FechaFin { get; set; }

    public decimal PrecioContratado { get; set; } 
    public bool Estado { get; set; } = true;

  
    public int IdUsuarioCreador { get; set; } 

    public Usuario usuario {get; set;}=new Usuario();
}}