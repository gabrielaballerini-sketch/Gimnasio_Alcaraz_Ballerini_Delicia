using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using MySql.Data.MySqlClient;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
    public class RepositorioClase : RepositorioBase, IRepositorioClase
    {
        public RepositorioClase(IConfiguration configuration)
            : base(configuration)
        {
        }

        // Consulta base: la clase con los datos de su actividad y su profesor
        private const string SelectBase = @"
            SELECT
                c.IdClase,
                c.IdActividad,
                c.IdProfesor,
                c.Dia,
                c.HoraInicio,
                c.HoraFin,
                c.Estado,

                a.Nombre AS Actividad_Nombre,
                a.AccesoLibre AS Actividad_AccesoLibre,

                p.Nombre AS Profesor_Nombre,
                p.Apellido AS Profesor_Apellido

            FROM clase c

            INNER JOIN actividad a
                ON a.IdActividad = c.IdActividad

            INNER JOIN profesor p
                ON p.IdProfesor = c.IdProfesor";


        public int Alta(Clase clase)
        {
            using var conexion = CrearConexion();

            var sql = @"
            INSERT INTO clase
                (IdActividad, IdProfesor, Dia, HoraInicio, HoraFin, Estado)
            VALUES
                (@IdActividad, @IdProfesor, @Dia, @HoraInicio, @HoraFin, @Estado);

            SELECT LAST_INSERT_ID();";

            using var comand = new MySqlCommand(sql, conexion);

            CargarParametros(comand, clase);

            conexion.Open();

            var id = Convert.ToInt32(comand.ExecuteScalar());

            clase.IdClase = id;

            return id;
        }


        public int Modificacion(Clase clase)
        {
            using var conexion = CrearConexion();

            var sql = @"
            UPDATE clase
            SET IdActividad = @IdActividad,
                IdProfesor = @IdProfesor,
                Dia = @Dia,
                HoraInicio = @HoraInicio,
                HoraFin = @HoraFin
            WHERE IdClase = @IdClase;";

            using var comand = new MySqlCommand(sql, conexion);

            CargarParametros(comand, clase);

            comand.Parameters.AddWithValue("@IdClase", clase.IdClase);

            conexion.Open();

            return comand.ExecuteNonQuery();
        }


        public int Baja(int id)
        {
            using var conexion = CrearConexion();

            var sql = @"
            UPDATE clase
            SET Estado = 0
            WHERE IdClase = @IdClase;";

            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue("@IdClase", id);

            conexion.Open();

            return comand.ExecuteNonQuery();
        }


        public int Reactivar(int id)
        {
            using var conexion = CrearConexion();

            var sql = @"
            UPDATE clase
            SET Estado = 1
            WHERE IdClase = @IdClase
              AND Estado = 0;";

            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue("@IdClase", id);

            conexion.Open();

            return comand.ExecuteNonQuery();
        }


        public Clase? ObtenerPorId(int id)
        {
            using var conexion = CrearConexion();

            var sql = SelectBase + @"
            WHERE c.IdClase = @IdClase;";

            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue("@IdClase", id);

            conexion.Open();

            using var read = comand.ExecuteReader();

            return read.Read() ? Mapear(read) : null;
        }


        public IList<Clase> ObtenerActivos(
            int pagina = 1,
            int tamPagina = 10)
        {
            return Buscar(true, null, null, pagina, tamPagina);
        }


        public IList<Clase> ObtenerInactivos(
            int pagina = 1,
            int tamPagina = 10)
        {
            return Buscar(false, null, null, pagina, tamPagina);
        }


        public int ObtenerCantidad(bool? soloActivos = true)
        {
            using var conexion = CrearConexion();

            var sql = @"
            SELECT COUNT(*)
            FROM clase
            WHERE
                (@Estado IS NULL
                OR Estado = @Estado);";

            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue(
                "@Estado",
                ValorODbNull(soloActivos));

            conexion.Open();

            return Convert.ToInt32(comand.ExecuteScalar());
        }


        public IList<Clase> Buscar(
            bool activas,
            int? idActividad,
            int? idProfesor,
            int pagina,
            int tamPagina)
        {
            if (pagina < 1)
                pagina = 1;

            if (tamPagina < 1)
                tamPagina = 10;

            IList<Clase> lista = new List<Clase>();

            using var conexion = CrearConexion();

            var sql = SelectBase + @"
            WHERE c.Estado = @Estado
              AND (@IdActividad IS NULL OR c.IdActividad = @IdActividad)
              AND (@IdProfesor IS NULL OR c.IdProfesor = @IdProfesor)
              " + FiltroVigentes(activas) + @"
            ORDER BY c.Dia, c.HoraInicio, c.IdClase
            LIMIT @tamPagina
            OFFSET @desplazamiento;";

            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue("@Estado", activas);
            comand.Parameters.AddWithValue("@IdActividad", ValorODbNull(idActividad));
            comand.Parameters.AddWithValue("@IdProfesor", ValorODbNull(idProfesor));
            comand.Parameters.AddWithValue("@tamPagina", tamPagina);
            comand.Parameters.AddWithValue("@desplazamiento", (pagina - 1) * tamPagina);

            conexion.Open();

            using var read = comand.ExecuteReader();

            while (read.Read())
            {
                lista.Add(Mapear(read));
            }

            return lista;
        }


        public int Contar(
            bool activas,
            int? idActividad,
            int? idProfesor)
        {
            using var conexion = CrearConexion();

            var sql = @"
            SELECT COUNT(*)
            FROM clase c
            INNER JOIN actividad a ON a.IdActividad = c.IdActividad
            INNER JOIN profesor p ON p.IdProfesor = c.IdProfesor
            WHERE c.Estado = @Estado
              AND (@IdActividad IS NULL OR c.IdActividad = @IdActividad)
              AND (@IdProfesor IS NULL OR c.IdProfesor = @IdProfesor)
              " + FiltroVigentes(activas) + ";";

            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue("@Estado", activas);
            comand.Parameters.AddWithValue("@IdActividad", ValorODbNull(idActividad));
            comand.Parameters.AddWithValue("@IdProfesor", ValorODbNull(idProfesor));

            conexion.Open();

            return Convert.ToInt32(comand.ExecuteScalar());
        }


        public IList<Clase> ObtenerGrilla()
        {
            IList<Clase> lista = new List<Clase>();

            using var conexion = CrearConexion();

            var sql = SelectBase + @"
            WHERE c.Estado = 1
              AND a.Estado = 1
              AND p.Estado = 1
            ORDER BY c.Dia, c.HoraInicio, c.IdClase;";

            using var comand = new MySqlCommand(sql, conexion);

            conexion.Open();

            using var read = comand.ExecuteReader();

            while (read.Read())
            {
                lista.Add(Mapear(read));
            }

            return lista;
        }


        public bool ExisteSuperposicionProfesor(
            int idProfesor,
            DiaSemana dia,
            TimeSpan inicio,
            TimeSpan fin,
            int idClaseExcluir)
        {
            using var conexion = CrearConexion();

            // Dos horarios se superponen si cada uno empieza antes de que termine el otro
            var sql = @"
            SELECT COUNT(*)
            FROM clase
            WHERE Estado = 1
              AND IdProfesor = @IdProfesor
              AND Dia = @Dia
              AND HoraInicio < @HoraFin
              AND HoraFin > @HoraInicio
              AND IdClase <> @IdClase;";

            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue("@IdProfesor", idProfesor);
            comand.Parameters.AddWithValue("@Dia", (int)dia);
            comand.Parameters.AddWithValue("@HoraInicio", inicio);
            comand.Parameters.AddWithValue("@HoraFin", fin);
            comand.Parameters.AddWithValue("@IdClase", idClaseExcluir);

            conexion.Open();

            return Convert.ToInt32(comand.ExecuteScalar()) > 0;
        }


        public int ContarActivasPorActividad(int idActividad)
        {
            using var conexion = CrearConexion();

            var sql = @"
            SELECT COUNT(*)
            FROM clase
            WHERE IdActividad = @IdActividad
              AND Estado = 1;";

            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue("@IdActividad", idActividad);

            conexion.Open();

            return Convert.ToInt32(comand.ExecuteScalar());
        }


        // En las clases activas solo se muestran las que tienen actividad y profesor vigentes
        private static string FiltroVigentes(bool activas)
        {
            return activas ? "AND a.Estado = 1 AND p.Estado = 1" : string.Empty;
        }


        private static void CargarParametros(
            MySqlCommand cmd,
            Clase clase)
        {
            cmd.Parameters.AddWithValue("@IdActividad", clase.IdActividad);
            cmd.Parameters.AddWithValue("@IdProfesor", clase.IdProfesor);
            cmd.Parameters.AddWithValue("@Dia", (int)clase.Dia);
            cmd.Parameters.AddWithValue("@HoraInicio", clase.HoraInicio);
            cmd.Parameters.AddWithValue("@HoraFin", clase.HoraFin);
            cmd.Parameters.AddWithValue("@Estado", clase.Estado);
        }


        private static Clase Mapear(MySqlDataReader r)
        {
            return new Clase
            {
                IdClase = r.GetInt32("IdClase"),
                IdActividad = r.GetInt32("IdActividad"),
                IdProfesor = r.GetInt32("IdProfesor"),
                Dia = (DiaSemana)r.GetInt32("Dia"),
                HoraInicio = r.GetTimeSpan("HoraInicio"),
                HoraFin = r.GetTimeSpan("HoraFin"),
                Estado = r.GetBoolean("Estado"),

                Actividad = new Actividad
                {
                    IdActividad = r.GetInt32("IdActividad"),
                    Nombre = r.GetString("Actividad_Nombre"),
                    AccesoLibre = r.GetBoolean("Actividad_AccesoLibre")
                },

                Profesor = new Profesor
                {
                    IdProfesor = r.GetInt32("IdProfesor"),
                    Nombre = r.GetString("Profesor_Nombre"),
                    Apellido = r.GetString("Profesor_Apellido")
                }
            };
        }
    }
}
