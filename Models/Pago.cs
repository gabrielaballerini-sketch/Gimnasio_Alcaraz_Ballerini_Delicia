using System.ComponentModel.DataAnnotations;
namespace Gimnasio_Alcaraz_Ballerini_Delicia.Models{
public class Pago
{
    public int IdPago { get; set; }
    
    public int IdMembresia { get; set; }
    public Membresia Membresia { get; set; } = null!;
    
    [Required(ErrorMessage ="Debe seleccionar un concepto")]
    public string? Concepto { get; set; } 
    
    public DateTime Fecha { get; set; }
    [Required(ErrorMessage ="Debe ingresar un importe")]
    public decimal Importe { get; set; }
    [Required(ErrorMessage ="Debe seleccionar un medio de pago")]
    public MedioPago MedioPago { get; set; }
    public bool Estado { get; set; } =true;

    // Auditoría
    public int IdUsuarioCreador { get; set; } // IdUsuario
    public int? IdUsuarioAnulador { get; set; } 
    public Usuario usuario {get; set;}=new Usuario();  // IdUsuario (opcional si se anula)
}
    public enum MedioPago
{
    Efectivo=1,
    Debito=2,
    Credito=3,
    Transferencia=4

}}