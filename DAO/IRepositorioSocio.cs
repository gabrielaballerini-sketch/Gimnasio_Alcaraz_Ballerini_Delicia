using Gimnasio_Alcaraz_Ballerini_Delicia.Models;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
    public interface IRepositorioSocio : IRepositorio<Socio>
    {
       bool ExisteDni(string dni, int? excluirId = null);
        Socio? ObtenerPorDni(string dni);
        Socio? ObtenerPorIdUsuario(int idUsuario);

        Socio ConvertirUsuarioEnSocio(int idUsuario, Socio datos);
        IList<Socio> Buscar(string texto, int limite = 10);
    
         int AltaConUsuario(Socio socio, Usuario usuario);
}}