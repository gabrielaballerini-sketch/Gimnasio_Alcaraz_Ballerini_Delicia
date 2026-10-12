using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.DAO;


namespace Gimnasio_Alcaraz_Ballerini_Delicia.Service
{
    public class SocioService
    {
        

        private readonly IRepositorioSocio _repo;
      

        public SocioService(IRepositorioSocio repo, IConfiguration config, IWebHostEnvironment env)
        {
            _repo = repo;
           
        }

        public ResultadoPaginado<Socio> ObtenerActivos(int pagina = 1, int tamPagina = 10)
        {
            if (pagina < 1) pagina = 1;
            if (tamPagina < 1) tamPagina = 10;

            return new ResultadoPaginado<Socio>
            {
                Items = _repo.ObtenerActivos(pagina, tamPagina),
                Pagina = pagina,
                TamPagina = tamPagina,
                Total = _repo.ObtenerCantidad(true)
            };
        }

        public ResultadoPaginado<Socio> ObtenerInactivos(int pagina = 1, int tamPagina = 10)
        {
            if (pagina < 1) pagina = 1;
            if (tamPagina < 1) tamPagina = 10;

            return new ResultadoPaginado<Socio>
            {
                Items = _repo.ObtenerInactivos(pagina, tamPagina),
                Pagina = pagina,
                TamPagina = tamPagina,
                Total = _repo.ObtenerCantidad(false)
            };
        }

        public Socio? Obtener(int id)
        {
            return _repo.ObtenerPorId(id);
        }

        public Socio Crear(Socio socio)
        {
            BorrarEspacios(socio);
            Validar(socio, null);

            socio.Estado = true;
            

           
          
                _repo.Alta(socio);
         
           
            return socio;
        }

        // Devuelve null si el socio no existe
        public Socio? Modificar(int id, Socio socio)
        {
            var existente = _repo.ObtenerPorId(id);
            if (existente == null) return null;

            BorrarEspacios(socio);
            Validar(socio, id);

            socio.IdSocio = id;
            socio.Estado = existente.Estado;   // el estado solo cambia con Baja / Reactivar
            
           
         
                _repo.Modificacion(socio);
            
        

         
           
            return socio;
        }

      
        public bool DarDeBaja(int id)
        {
            return _repo.Baja(id) > 0;
        }

        // Devuelve false si el socio no existe o ya estaba activo
        public bool Reactivar(int id)
        {
            return _repo.Reactivar(id) > 0;
        }

        private static void BorrarEspacios(Socio s)
        {
            s.Nombre = s.Nombre?.Trim();
            s.Apellido = s.Apellido?.Trim();
            s.Dni = s.Dni?.Trim();
            s.Telefono = s.Telefono?.Trim();
            s.Domicilio = s.Domicilio?.Trim();
        }

        private void Validar(Socio s, int? idExistente)
        {
            if (string.IsNullOrEmpty(s.Dni))
                throw new ReglaNegocioException("El DNI es obligatorio.");

            if (_repo.ExisteDni(s.Dni, idExistente))
                throw new ReglaNegocioException("Ya existe un socio con ese DNI.");

           
        }

       
           

        public IList<Socio> Buscar(string? texto, int limite = 10)
        {
            texto = texto?.Trim();

            if (string.IsNullOrEmpty(texto) || texto.Length < 2)
                return new List<Socio>();

            if (limite < 1 || limite > 20) limite = 10;

            return _repo.Buscar(texto, limite);
        }
    }
}