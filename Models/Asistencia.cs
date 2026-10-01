using System.ComponentModel.DataAnnotations;
namespace Gimnasio_Alcaraz_Ballerini_Delicia.Models{
public class Asistencia
{
    public int IdAsistencia { get; set; }
    
    public int IdSocio { get; set; }
    public Socio Socio { get; set; }=new Socio();
    

    public int? IdClase { get; set; }
    public Clase? Clase { get; set; }
    
  
    public DateTime FechaHoraIngreso { get; set; }
}}