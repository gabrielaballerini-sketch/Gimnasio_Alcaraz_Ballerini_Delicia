using System.ComponentModel.DataAnnotations;
namespace Gimnasio_Alcaraz_Ballerini_Delicia.Models{
public class Usuario
{
    public int IdUsuario { get; set; }
    [Required(ErrorMessage ="Debe ingresar un Email")]
    [EmailAddress]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
    ErrorMessage = " (ejemplo: usuario@dominio.com)")]
    public string? Email { get; set; } 

    [StringLength(50, ErrorMessage = "El nombre de usuario no puede superar los 50 caracteres")]
     public string? NombreUsuario { get; set; }

   
    [Required(ErrorMessage ="Debe ingresar un password")]
    [RegularExpression(@"^[a-zA-ZñÑ\d\s]{6,20}$",
    ErrorMessage = "El password debe contener minimo 6 caracteres ")]
     public string? Password { get; set; } 
     
    public Roles Roles { get; set; }

    public bool Estado { get; set; } = true;
    public string? Avatar { get; set; } 

    public IFormFile? avatarFile {get;set;}



}

public enum Roles
{
    Socio=1,
    Empleado=2,
    Administrador=3
}}