using Gimnasio_Alcaraz_Ballerini_Delicia.Models;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
    public interface IRepositorioMembresia : IRepositorio<Membresia>
    {
        IList<Membresia> ObtenerActivosPorSocio(
            int idSocio,
            int pagina = 1,
            int tamPagina = 10);
    }
}