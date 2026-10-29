using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.Service;
using MySql.Data.MySqlClient;


namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
    public class RepositorioAsistencia : RepositorioBase, IRepositorioAsistencia
    {
        public RepositorioAsistencia(IConfiguration configuration)
            : base(configuration)
        {
        }

        // Consulta base: la asistencia con el socio y, si fue a una clase, su actividad
        private const string SelectBase = @"
            SELECT
                a.IdAsistencia,
                a.IdSocio,
                a.IdClase,
                a.FechaIngreso,
                a.Tipo,

                s.Nombre   AS Socio_Nombre,
                s.Apellido AS Socio_Apellido,
                s.Dni      AS Socio_Dni,

                c.Dia        AS Clase_Dia,
                c.HoraInicio AS Clase_HoraInicio,
                c.HoraFin    AS Clase_HoraFin,
                ac.Nombre    AS Actividad_Nombre

            FROM asistencia a

            INNER JOIN socio s
                ON s.IdSocio = a.IdSocio

            LEFT JOIN clase c
                ON c.IdClase = a.IdClase

            LEFT JOIN actividad ac
                ON ac.IdActividad = c.IdActividad";

        private const string Filtros = @"
            WHERE (@Desde IS NULL OR a.FechaIngreso >= @Desde)
              AND (@Hasta IS NULL OR a.FechaIngreso < @Hasta)
              AND (@IdSocio IS NULL OR a.IdSocio = @IdSocio)";


        public int Alta(Asistencia asistencia)
        {
            using var conexion = CrearConexion();

            var sql = @"
            INSERT INTO asistencia
                (IdSocio, IdClase, FechaIngreso, Tipo)
            VALUES
                (@IdSocio, @IdClase, @FechaIngreso, @Tipo);

            SELECT LAST_INSERT_ID();";

            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue("@IdSocio", asistencia.IdSocio);
            comand.Parameters.AddWithValue("@IdClase", ValorODbNull(asistencia.IdClase));
            comand.Parameters.AddWithValue("@FechaIngreso", asistencia.FechaHoraIngreso);
            comand.Parameters.AddWithValue("@Tipo", (int)asistencia.Tipo);

            conexion.Open();

            asistencia.IdAsistencia = Convert.ToInt32(comand.ExecuteScalar());

            return asistencia.IdAsistencia;
        }


        public Membresia? ObtenerMembresiaVigente(int idSocio, DateTime fecha)
        {
            using var conexion = CrearConexion();

            var sql = @"
            SELECT
                m.IdMembresia,
                m.IdSocio,
                m.IdPlan,
                m.FechaInicio,
                m.FechaFin,
                m.Pagada,

                p.Nombre                 AS Plan_Nombre,
                p.UtilizacionesMensuales AS Plan_Utilizaciones,
                p.EsIlimitado            AS Plan_EsIlimitado

            FROM membresia m

            INNER JOIN plan p
                ON p.IdPlan = m.IdPlan

            WHERE m.IdSocio = @IdSocio
              AND m.Estado = 1
              AND m.Pagada = 1
              AND m.FechaInicio <= @Fecha
              AND m.FechaFin >= @Fecha

            ORDER BY m.FechaFin DESC
            LIMIT 1;";

            using var comand = new MySqlCommand(sql, conexion);
            comand.Parameters.AddWithValue("@IdSocio", idSocio);
            comand.Parameters.AddWithValue("@Fecha", fecha.Date);

            conexion.Open();

            using var r = comand.ExecuteReader();

            if (!r.Read())
                return null;

            return new Membresia
            {
                IdMembresia = r.GetInt32("IdMembresia"),
                IdSocio = r.GetInt32("IdSocio"),
                IdPlan = r.GetInt32("IdPlan"),
                FechaInicio = r.GetDateTime("FechaInicio"),
                FechaFin = r.GetDateTime("FechaFin"),
                Pagada = r.GetBoolean("Pagada"),
                Plan = new Plan
                {
                    IdPlan = r.GetInt32("IdPlan"),
                    Nombre = r.GetString("Plan_Nombre"),
                    UtilizacionesMensuales = LeerIntNull(r, "Plan_Utilizaciones"),

                    // En la base EsIlimitado admite NULL: se toma como "no ilimitado"
                    EsIlimitado = LeerBool(r, "Plan_EsIlimitado", false)
                }
            };
        }


        public int ContarEnPeriodo(int idSocio, DateTime desde, DateTime hasta)
        {
            using var conexion = CrearConexion();

            var sql = @"
            SELECT COUNT(*)
            FROM asistencia
            WHERE IdSocio = @IdSocio
              AND FechaIngreso >= @Desde
              AND FechaIngreso < @Hasta;";

            using var comand = new MySqlCommand(sql, conexion);
            comand.Parameters.AddWithValue("@IdSocio", idSocio);
            comand.Parameters.AddWithValue("@Desde", desde);
            comand.Parameters.AddWithValue("@Hasta", hasta);

            conexion.Open();

            return Convert.ToInt32(comand.ExecuteScalar());
        }


        public bool ExisteEnRango(int idSocio, int? idClase, DateTime desde, DateTime hasta)
        {
            using var conexion = CrearConexion();

            var sql = @"
            SELECT COUNT(*)
            FROM asistencia
            WHERE IdSocio = @IdSocio
              AND FechaIngreso >= @Desde
              AND FechaIngreso < @Hasta
              AND ((@IdClase IS NULL AND IdClase IS NULL) OR IdClase = @IdClase);";

            using var comand = new MySqlCommand(sql, conexion);
            comand.Parameters.AddWithValue("@IdSocio", idSocio);
            comand.Parameters.AddWithValue("@Desde", desde);
            comand.Parameters.AddWithValue("@Hasta", hasta);
            comand.Parameters.AddWithValue("@IdClase", ValorODbNull(idClase));

            conexion.Open();

            return Convert.ToInt32(comand.ExecuteScalar()) > 0;
        }


        public bool ExisteSuperposicion(int idSocio,DateTime desde,DateTime hasta,TimeSpan inicio,TimeSpan fin)
        {
            using var conexion = CrearConexion();

            // Dos horarios se superponen si cada uno empieza antes de que termine el otro
            var sql = @"
            SELECT COUNT(*)
            FROM asistencia a
            INNER JOIN clase c
                ON c.IdClase = a.IdClase
            WHERE a.IdSocio = @IdSocio
              AND a.FechaIngreso >= @Desde
              AND a.FechaIngreso < @Hasta
              AND c.HoraInicio < @Fin
              AND c.HoraFin > @Inicio;";

            using var comand = new MySqlCommand(sql, conexion);
            comand.Parameters.AddWithValue("@IdSocio", idSocio);
            comand.Parameters.AddWithValue("@Desde", desde);
            comand.Parameters.AddWithValue("@Hasta", hasta);
            comand.Parameters.AddWithValue("@Inicio", inicio);
            comand.Parameters.AddWithValue("@Fin", fin);

            conexion.Open();

            return Convert.ToInt32(comand.ExecuteScalar()) > 0;
        }


        public IList<Asistencia> Listar(
            DateTime? desde,
            DateTime? hasta,
            int? idSocio,
             int pagina = PaginacionParametros.PaginaPredeterminada,
             int tamPagina = PaginacionParametros.TamPaginaPredeterminado)
        {
            
            IList<Asistencia> lista = new List<Asistencia>();

            using var conexion = CrearConexion();

            var sql = SelectBase + Filtros + @"
            ORDER BY a.FechaIngreso DESC, a.IdAsistencia DESC
            LIMIT @Limite OFFSET @Offset;";

            using var comand = new MySqlCommand(sql, conexion);

            CargarFiltros(comand, desde, hasta, idSocio);
            comand.Parameters.AddWithValue("@Limite", tamPagina);
            comand.Parameters.AddWithValue("@Offset", (pagina - 1) * tamPagina);

            conexion.Open();

            using var read = comand.ExecuteReader();

            while (read.Read())
            {
                lista.Add(Mapear(read));
            }

            return lista;
        }


        public int Contar(DateTime? desde, DateTime? hasta, int? idSocio)
        {
            using var conexion = CrearConexion();

            var sql = "SELECT COUNT(*) FROM asistencia a" + Filtros + ";";

            using var comand = new MySqlCommand(sql, conexion);

            CargarFiltros(comand, desde, hasta, idSocio);

            conexion.Open();

            return Convert.ToInt32(comand.ExecuteScalar());
        }


        private static void CargarFiltros(
            MySqlCommand comand,
            DateTime? desde,
            DateTime? hasta,
            int? idSocio)
        {
            comand.Parameters.AddWithValue("@Desde", ValorODbNull(desde));
            comand.Parameters.AddWithValue("@Hasta", ValorODbNull(hasta));
            comand.Parameters.AddWithValue("@IdSocio", ValorODbNull(idSocio));
        }


        private static Asistencia Mapear(MySqlDataReader r)
        {
            var asistencia = new Asistencia
            {
                IdAsistencia = r.GetInt32("IdAsistencia"),
                IdSocio = r.GetInt32("IdSocio"),
                IdClase = LeerIntNull(r, "IdClase"),
                FechaHoraIngreso = r.GetDateTime("FechaIngreso"),
                Tipo = (TipoAsistencia)r.GetInt32("Tipo"),

                Socio = new Socio
                {
                    IdSocio = r.GetInt32("IdSocio"),
                    Nombre = r.GetString("Socio_Nombre"),
                    Apellido = r.GetString("Socio_Apellido"),
                    Dni = r.GetString("Socio_Dni")
                }
            };

            if (asistencia.IdClase != null)
            {
                asistencia.Clase = new Clase
                {
                    IdClase = asistencia.IdClase.Value,
                    Dia = (DiaSemana)r.GetInt32("Clase_Dia"),
                    HoraInicio = r.GetTimeSpan("Clase_HoraInicio"),
                    HoraFin = r.GetTimeSpan("Clase_HoraFin"),
                    Actividad = new Actividad
                    {
                        Nombre = r.GetString("Actividad_Nombre")
                    }
                };
            }

            return asistencia;
        }
    }
}