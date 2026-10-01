public class Usuario
{
    public int IdUsuario { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Roles Roles { get; set; }
}

public enum Roles
{
    Socio=1,
    Empleado=3,
    Administrador=3
}