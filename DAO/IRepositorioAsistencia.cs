using Gimnasio_Alcaraz_Ballerini_Delicia.Models;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
    public interface IRepositorioAsistencia
    {
        int Alta(Asistencia asistencia);




        // Membresía activa del socio que cubre la fecha, con los datos de su plan
        Membresia? ObtenerMembresiaVigente(int idSocio, DateTime fecha);

       
         int ContarEnPeriodo(int idSocio, DateTime desde, DateTime hasta);
        

        // ¿Ya hay un ingreso del socio a esa clase (o libre, si idClase es null) en el rango?
        bool ExisteEnRango(int idSocio, int? idClase, DateTime desde, DateTime hasta);
     bool ExisteSuperposicion(int idSocio,DateTime desde,DateTime hasta,TimeSpan inicio,TimeSpan fin);
        

        // Todos los filtros son opcionales; el rango es [desde, hasta)
        IList<Asistencia> Listar(
            DateTime? desde,
            DateTime? hasta,
            int? idSocio,
            int pagina,
            int tamPagina);

        int Contar(
            DateTime? desde,
            DateTime? hasta,
            int? idSocio
            );
    }
}