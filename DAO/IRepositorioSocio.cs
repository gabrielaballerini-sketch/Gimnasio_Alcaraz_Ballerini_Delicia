using Gimnasio_Alcaraz_Ballerini_Delicia.Models;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
    public interface IRepositorioSocio : IRepositorio<Socio>
    {
        bool ExisteDni(string dni, int? excluirId = null);
        Socio? ObtenerPorDni(string dni);
        IList<Socio> Buscar(string texto, int limite = 10);

    }
}