using Gimnasio_Alcaraz_Ballerini_Delicia.Models;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
    public interface IRepositorioMembresia : IRepositorio<Membresia>
    {
        Membresia? ObtenerParaCobroPorDni(string dni);

       
        IList<Membresia> BuscarPorDni(
            string dni,
            bool soloActivas,
            int pagina = 1,
            int tamPagina = 10);

        int ObtenerCantidadPorDni(string dni, bool soloActivas);


        bool ExistePorSocio(int idSocio);
        
        IList<Membresia> ObtenerActivosPorSocio(
            int idSocio,
            int pagina = 1,
            int tamPagina = 10);

      IList<Membresia> ObtenerProximasAVencer(
    DateTime hoy,
    DateTime fechaLimite,
    int pagina,
    int tamPagina
);
int ObtenerCantidadProximasAVencer(
    DateTime hoy,
    DateTime fechaLimite
);


      IList<Membresia> ObtenerVigentesEnFecha(
    DateTime fecha,
    int pagina,
    int tamPagina);
    int ObtenerCantidadVigentesEnFecha(DateTime fecha);

    }
}