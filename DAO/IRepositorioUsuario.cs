using Gimnasio_Alcaraz_Ballerini_Delicia.Models;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
    public interface IRepositorioUsuario : IRepositorio<Usuario>
    {
        bool ExisteEmail(string email, int? excluirId = null);

        Usuario? ObtenerPorEmail(string email);
    }
}