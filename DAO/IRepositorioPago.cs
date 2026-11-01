using Gimnasio_Alcaraz_Ballerini_Delicia.Models;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
    public interface IRepositorioPago
    {
        int Alta(Pago pago);

        int RegistrarPagoYHabilitarMembresia(
            Pago pago,
            DateTime fechaInicio,
            DateTime fechaFin);

        Pago? ObtenerPorId(int id);

        IList<Pago> ObtenerTodos(
            int pagina = 1,
            int tamPagina = 10);

        IList<Pago> ObtenerPorSocio(
            int idUsuario,
            int pagina = 1,
            int tamPagina = 10);

        int ObtenerCantidad(bool? soloActivos = true);

        int Anular(int idPago, int idUsuarioAnulador);

        IList<(string Plan, int Cantidad, decimal Total)> ObtenerRecaudacionPorPlan(

        DateTime desde, DateTime hastaExclusivo, int? idPlan, int? medioPago);

        IList<(int Medio, int Cantidad, decimal Total)> ObtenerRecaudacionPorMedio(

        DateTime desde, DateTime hastaExclusivo, int? idPlan, int? medioPago);
      
        int ObtenerCantidadPorUsuario(int idUsuario);

    }
}