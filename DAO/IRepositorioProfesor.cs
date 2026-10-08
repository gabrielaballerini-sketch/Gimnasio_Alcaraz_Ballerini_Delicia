using Gimnasio_Alcaraz_Ballerini_Delicia.Models;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
    public interface IRepositorioProfesor : IRepositorio<Profesor>
    {
        bool ExisteDni(string dni, int? excluirId = null);

        Profesor? ObtenerPorDni(string dni);
    }
}