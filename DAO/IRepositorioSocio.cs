using Gimnasio_Alcaraz_Ballerini_Delicia.Models;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
    public interface IRepositorioSocio : IRepositorio<Socio>
    {
       bool ExisteDni(string dni, int? excluirId = null);
        Socio? ObtenerPorDni(string dni);
        Socio? ObtenerPorIdUsuario(int idUsuario);
        IList<Socio> Buscar(string texto, int limite = 10, bool soloSinUsuario = false);
        int VincularUsuario(int idSocio, int idUsuario);
        int DesvincularUsuario(int idUsuario);

         int AltaConUsuario(Socio socio, Usuario usuario);
}}