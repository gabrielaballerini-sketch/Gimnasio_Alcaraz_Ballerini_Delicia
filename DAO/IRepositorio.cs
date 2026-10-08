namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{

    public interface IRepositorio<T>
    {
        int Alta(T entidad);

        int Baja(int id);

        int Modificacion(T entidad);

        int Reactivar(int id);

        IList<T> ObtenerActivos(
            int pagina = 1,
            int tamPagina = 10
        );

        IList<T> ObtenerInactivos(
            int pagina = 1,
            int tamPagina = 10
        );


        int ObtenerCantidad(bool? soloActivos = true);

        T? ObtenerPorId(int id);
    }
}
