using Gimnasio_Alcaraz_Ballerini_Delicia.DAO;
using Gimnasio_Alcaraz_Ballerini_Delicia.Models;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Service
{
    // Situación de un socio frente al acceso: qué plan tiene y cuánto usó


    public class AsistenciaService
    {
        private readonly IRepositorioAsistencia _repo;
        private readonly IRepositorioSocio _repoSocio;
        private readonly IRepositorioClase _repoClase;

        public AsistenciaService(
            IRepositorioAsistencia repo,
            IRepositorioSocio repoSocio,
            IRepositorioClase repoClase)
        {
            _repo = repo;
            _repoSocio = repoSocio;
            _repoClase = repoClase;
        }


        public EstadoAccesoAsistencia ObtenerAcceso(int idSocio)
        {
            var socio = _repoSocio.ObtenerPorId(idSocio);

            if (socio == null)
            {
                throw new ReglaNegocioException("El socio no existe.");
            }

            var acceso = new EstadoAccesoAsistencia
            {
                IdSocio = socio.IdSocio,
                Nombre = socio.Nombre ?? "",
                Apellido = socio.Apellido ?? "",
                Dni = socio.Dni ?? ""
            };

            if (!socio.Estado)
            {
                acceso.Motivo = "El socio está dado de baja.";
                return acceso;
            }




            var membresia = _repo.ObtenerMembresiaVigente(
       idSocio, DateTime.Today);

            if (membresia == null)
            {
                acceso.Motivo =
                    "El socio no tiene una membresía vigente.";
                return acceso;
            }

            if (!membresia.Pagada)
            {
                acceso.Motivo = "El período de la membresía está pendiente de pago.";
                return acceso;
            }

            var tieneLimite = !membresia.Plan.EsIlimitado &&
                              membresia.Plan.UtilizacionesMensuales.HasValue;

            acceso.IdMembresia = membresia.IdMembresia;
            acceso.Plan = membresia.Plan.Nombre;
            acceso.VenceEl = membresia.FechaFin;
            acceso.Ilimitado = !tieneLimite;
            acceso.Limite = tieneLimite ? membresia.Plan.UtilizacionesMensuales : null;


            var inicioMes = new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month,
                1);

            var inicioMesSiguiente = inicioMes.AddMonths(1);

            acceso.Usadas = _repo.ContarEnPeriodo(
                idSocio,
                inicioMes,
                inicioMesSiguiente);



            if (tieneLimite && acceso.Usadas >= acceso.Limite)
            {
                acceso.Motivo =
                    $"Alcanzó el límite de {acceso.Limite} utilizaciones de su plan.";
                return acceso;
            }

            acceso.PuedeIngresar = true;

            return acceso;
        }



        public Asistencia Registrar(int idSocio, int? idClase, int idUsuario)
        {
            var acceso = ObtenerAcceso(idSocio);

            if (!acceso.PuedeIngresar)
            {
                throw new ReglaNegocioException(
                    acceso.Motivo ?? "El socio no puede ingresar.");
            }

            var ahora = DateTime.Now;

            Clase? claseSeleccionada = null;

            if (idClase.HasValue)
            {
                claseSeleccionada = _repoClase.ObtenerPorId(idClase.Value);

                if (claseSeleccionada == null || !claseSeleccionada.Estado)
                {
                    throw new ReglaNegocioException(
                        "La clase seleccionada no existe o está inactiva.");
                }

                var hoy = (DiaSemana)(((int)ahora.DayOfWeek + 6) % 7 + 1);

                if (claseSeleccionada.Dia != hoy)
                {
                    throw new ReglaNegocioException(
                        "La clase seleccionada no se dicta hoy.");
                }

                if (ahora.TimeOfDay < claseSeleccionada.HoraInicio ||
                    ahora.TimeOfDay >= claseSeleccionada.HoraFin)
                {
                    throw new ReglaNegocioException(
                        "La clase no está en horario. Solo se puede registrar el ingreso mientras se dicta.");
                }

                if (_repo.ExisteSuperposicion(
                    idSocio,
                    ahora.Date,
                    ahora.Date.AddDays(1),
                    claseSeleccionada.HoraInicio,
                    claseSeleccionada.HoraFin))
                {
                    throw new ReglaNegocioException(
                        "El socio ya tiene una asistencia registrada en un horario superpuesto.");
                }
            }
            else if (_repo.ExisteEnRango(
                idSocio, null, ahora.Date, ahora.Date.AddDays(1)))
            {
                throw new ReglaNegocioException(
                    "El socio ya registró su ingreso libre hoy.");
            }

            if (idClase.HasValue &&
                _repo.ExisteEnRango(
                    idSocio, idClase, ahora.Date, ahora.Date.AddDays(1)))
            {
                throw new ReglaNegocioException(
                    "El socio ya registró su ingreso a esta clase hoy.");
            }

            var asistencia = new Asistencia
            {
                IdSocio = idSocio,
                IdClase = idClase,
                Tipo = idClase.HasValue
                    ? TipoAsistencia.Clase
                    : TipoAsistencia.Musculacion,
                FechaHoraIngreso = ahora
            };

            _repo.Alta(asistencia);

            return asistencia;
        }



        public ResultadoPaginado<Asistencia> Listar(
            DateTime? desde,
            DateTime? hasta,
            int? idSocio,
            int pagina = 1,
            int tamPagina = 10)
        {
            if (pagina < 1)
                pagina = 1;

            if (tamPagina < 1)
                tamPagina = 10;

            return new ResultadoPaginado<Asistencia>
            {
                Items = _repo.Listar(desde, hasta, idSocio, pagina, tamPagina),
                Pagina = pagina,
                TamPagina = tamPagina,
                Total = _repo.Contar(desde, hasta, idSocio)
            };
        }
    }
}