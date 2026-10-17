using Gimnasio_Alcaraz_Ballerini_Delicia.Models;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
    public interface IRepositorioActividadArchivo
    {
        int Alta(ActividadArchivo archivo);

        int Eliminar(int idArchivo);

        ActividadArchivo? ObtenerPorId(int idArchivo);

        IList<ActividadArchivo> ObtenerPorActividad(int idActividad);

        int Contar(int idActividad);
    }
}
