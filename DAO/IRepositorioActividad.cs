using Gimnasio_Alcaraz_Ballerini_Delicia.Models;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
    public interface IRepositorioActividad : IRepositorio<Actividad>
    {
        // Solo activas. Si soloProgramables, excluye las de acceso libre.
        IList<Actividad> Buscar(string? texto, bool soloProgramables, int limite = 10);
    }
}