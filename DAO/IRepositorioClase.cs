using Gimnasio_Alcaraz_Ballerini_Delicia.Models;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
    public interface IRepositorioClase : IRepositorio<Clase>
    {
        // Listado con filtros opcionales por actividad y profesor
        IList<Clase> Buscar(
            bool activas,
            int? idActividad,
            int? idProfesor,
            int pagina,
            int tamPagina);

        int Contar(
            bool activas,
            int? idActividad,
            int? idProfesor);

        // Todas las clases vigentes de la semana, ordenadas por día y horario
        IList<Clase> ObtenerGrilla();

        // ¿El profesor ya tiene otra clase activa ese día que se superponga con el horario?
        bool ExisteSuperposicionProfesor(
            int idProfesor,
            DiaSemana dia,
            TimeSpan inicio,
            TimeSpan fin,
            int idClaseExcluir);

        int ContarActivasPorActividad(int idActividad);
    }
}
