public class ResultadoPaginado<T>
{
    public IList<T> Items { get; set; } = new List<T>();
    public int Pagina { get; set; }
    public int TamPagina { get; set; }
    public int Total { get; set; }

    // Siempre al menos 1, para mostrar "Página 1 de 1" cuando no hay registros
    public int TotalPaginas => Math.Max(1, (int)Math.Ceiling(Total / (double)TamPagina));
}