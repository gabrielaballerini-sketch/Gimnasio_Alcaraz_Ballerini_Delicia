using System.ComponentModel.DataAnnotations;
namespace Gimnasio_Alcaraz_Ballerini_Delicia.Models{
public class Socio
{
    public int IdSocio { get; set; }

    [Required(ErrorMessage ="Debe ingresar un nombre ")]
    [RegularExpression(@"^[a-zA-ZñÑ\s]{3,20}$",
    ErrorMessage = "El nombre solo puede tener letras y espacios")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage ="Debe ingresar un Apellido ")]
    [RegularExpression(@"^[a-zA-ZñÑ\s]{3,20}$",
    ErrorMessage = "El Apellido solo puede tener letras y espacios")]
    public string? Apellido { get; set; } 

    [Required(ErrorMessage ="Debe ingresar un Dni ")]
    [RegularExpression(@"^[\d]{7,10}$",
    ErrorMessage = "El dni debe tener entre 7 y 10 digitos")]
    public string? Dni { get; set; } 

     [Required(ErrorMessage ="Debe ingresar un Telefono ")]
    [RegularExpression(@"^[\d]{10,15}$",
    ErrorMessage = "El telefono debe tener entre 10 y 15 digitos")]
    public string? Telefono { get; set; }

     [Required(ErrorMessage ="Debe ingresar un Domicilio ")]
    [RegularExpression(@"^[a-zA-ZñÑ\d\s]{5,20}$",
    ErrorMessage = "El domicilio puede tener letras y numeros ")]
    public string? Domicilio { get; set; } 
    
    public string? Avatar { get; set; } 

    public IFormFile? avatarFile {get;set;}
    public bool Estado { get; set; } = true;

}}