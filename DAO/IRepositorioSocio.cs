namespace Gimnasio_Alcaraz_Ballerini_Delicia.Models
{
    public interface IRepositorioSocio : IRepositorio<Socio>
    {
         bool ExisteDni(string dni, int? excluirId = null);
         Socio? ObtenerPorDni(string dni);
        
    }
}