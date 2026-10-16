

public class ReglaNegocioException : Exception
{
    public List<string> Errores { get; } = new();

    public ReglaNegocioException(string mensaje) : base(mensaje) { }

    public ReglaNegocioException(string mensaje, List<string> errores) : base(mensaje)
    {
        Errores = errores;
    }
}
 