
namespace Gimnasio_Alcaraz_Ballerini_Delicia.Service
{
    public static class PaginacionParametros
    {
        public const int PaginaPredeterminada = 1;
        public const int TamPaginaPredeterminado = 10;
        public const int TamPaginaMaximo = 100;

        public static (int Pagina, int TamPagina) Normalizar(
            int pagina,
            int tamPagina)
        {
            pagina = Math.Max(1, pagina);

            tamPagina = Math.Clamp(
                tamPagina,
                1,
                TamPaginaMaximo
            );

            return (pagina, tamPagina);
        }

      
    }
}
