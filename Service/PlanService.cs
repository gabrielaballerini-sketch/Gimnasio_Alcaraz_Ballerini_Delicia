using Gimnasio_Alcaraz_Ballerini_Delicia.Models;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Service
{
    public class PlanService
    {
        private readonly IRepositorio<Plan> _repo;

        public PlanService(IRepositorio<Plan> repo)
        {
            _repo = repo;
        }

        public ResultadoPaginado<Plan> ObtenerActivos(
            int pagina = 1,
            int tamPagina = 10)
        {
            if (pagina < 1)
                pagina = 1;

            if (tamPagina < 1)
                tamPagina = 10;

            return new ResultadoPaginado<Plan>
            {
                Items = _repo.ObtenerActivos(
                    pagina,
                    tamPagina
                ),

                Pagina = pagina,

                TamPagina = tamPagina,

                Total = _repo.ObtenerCantidad(true)
            };
        }

        public ResultadoPaginado<Plan> ObtenerInactivos(
            int pagina = 1,
            int tamPagina = 10)
        {
            if (pagina < 1)
                pagina = 1;

            if (tamPagina < 1)
                tamPagina = 10;

            return new ResultadoPaginado<Plan>
            {
                Items = _repo.ObtenerInactivos(
                    pagina,
                    tamPagina
                ),

                Pagina = pagina,

                TamPagina = tamPagina,

                Total = _repo.ObtenerCantidad(false)
            };
        }

        public Plan? Obtener(int id)
        {
            return _repo.ObtenerPorId(id);
        }

        public Plan Crear(Plan plan)
        {
            BorrarEspacios(plan);

            Validar(plan);

            plan.Estado = true;

            _repo.Alta(plan);

            return plan;
        }

        public Plan? Modificar(
            int id,
            Plan plan)
        {
            var existente =
                _repo.ObtenerPorId(id);

            if (existente == null)
                return null;

            BorrarEspacios(plan);

            Validar(plan);

            plan.IdPlan = id;

            plan.Estado =
                existente.Estado;

            _repo.Modificacion(plan);

            return plan;
        }

        public bool DarDeBaja(int id)
        {
            return _repo.Baja(id) > 0;
        }

        public bool Reactivar(int id)
        {
            return _repo.Reactivar(id) > 0;
        }

        private static void BorrarEspacios(
            Plan plan)
        {
            plan.Nombre =
                plan.Nombre?.Trim();
        }

       private void Validar(Plan plan)
{
    if (string.IsNullOrWhiteSpace(plan.Nombre))
        throw new ReglaNegocioException(
            "El nombre del plan es obligatorio.");

    if (plan.Precio <= 0)
        throw new ReglaNegocioException(
            "El precio debe ser mayor a cero.");

    if (plan.EsIlimitado)
    {
        // Es ilimitado.
        // UtilizacionesMensuales debe ser null.
        return;
    }

    // Si llegó acá, NO es ilimitado.
    // Por lo tanto tiene que tener un número válido.
    if (!plan.UtilizacionesMensuales.HasValue ||
        plan.UtilizacionesMensuales.Value <= 0)
    {
        throw new ReglaNegocioException(
            "Las utilizaciones mensuales deben ser mayores a cero.");
    }
}
        
    }
}