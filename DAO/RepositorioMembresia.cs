using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using MySql.Data.MySqlClient;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
    public class RepositorioMembresia : RepositorioBase, IRepositorioMembresia
    {
        public RepositorioMembresia(IConfiguration configuration)
            : base(configuration)
        {
        }

        public int Alta(Membresia membresia)
        {
            using var conexion = CrearConexion();

            const string sql = @"
                INSERT INTO membresia
                (
                    IdSocio,
                    IdPlan,
                    FechaInicio,
                    FechaFin,
                    PrecioContratado,
                    Estado,
                    Pagada,
                    IdUsuarioCreador
                )
                VALUES
                (
                    @IdSocio,
                    @IdPlan,
                    @FechaInicio,
                    @FechaFin,
                    @PrecioContratado,
                    @Estado,
                    @Pagada,
                    @IdUsuarioCreador
                );

                SELECT LAST_INSERT_ID();";

            using var comando = new MySqlCommand(sql, conexion);

            CargarParametros(comando, membresia);

            conexion.Open();

            membresia.IdMembresia =
                Convert.ToInt32(comando.ExecuteScalar());

            return membresia.IdMembresia;
        }

        public int Modificacion(Membresia membresia)
        {
            using var conexion = CrearConexion();

            const string sql = @"
                UPDATE membresia
                SET IdSocio = @IdSocio,
                    IdPlan = @IdPlan,
                    FechaInicio = @FechaInicio,
                    FechaFin = @FechaFin,
                    PrecioContratado = @PrecioContratado,
                    Estado = @Estado,
                    Pagada = @Pagada
                WHERE IdMembresia = @IdMembresia;";

            using var comando = new MySqlCommand(sql, conexion);

            CargarParametros(comando, membresia);
            comando.Parameters.AddWithValue(
                "@IdMembresia", membresia.IdMembresia);

            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        public int Baja(int id)
        {
            using var conexion = CrearConexion();

            const string sql = @"
                UPDATE membresia
                SET Estado = 0
                WHERE IdMembresia = @IdMembresia;";

            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@IdMembresia", id);

            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        public int Reactivar(int id)
        {
            using var conexion = CrearConexion();

            const string sql = @"
                UPDATE membresia
                SET Estado = 1
                WHERE IdMembresia = @IdMembresia
                  AND Estado = 0;";

            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@IdMembresia", id);

            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        public Membresia? ObtenerPorId(int id)
        {
            using var conexion = CrearConexion();

            const string sql = @"
                SELECT
                    m.IdMembresia,
                    m.IdSocio,
                    m.IdPlan,
                    m.FechaInicio,
                    m.FechaFin,
                    m.PrecioContratado,
                    m.Estado,
                    m.Pagada,
                    m.IdUsuarioCreador,

                    s.IdSocio AS Socio_IdSocio,
                    s.Nombre AS Socio_Nombre,
                    s.Apellido AS Socio_Apellido,
                    s.Dni AS Socio_Dni,

                    p.IdPlan AS Plan_IdPlan,
                    p.Nombre AS Plan_Nombre,
                    p.Precio AS Plan_Precio,
                    p.UtilizacionesMensuales AS Plan_UtilizacionesMensuales

                FROM membresia m
                INNER JOIN socio s ON s.IdSocio = m.IdSocio
                INNER JOIN plan p ON p.IdPlan = m.IdPlan
                WHERE m.IdMembresia = @IdMembresia;";

            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@IdMembresia", id);

            conexion.Open();

            using var reader = comando.ExecuteReader();

            return reader.Read() ? Mapear(reader) : null;
        }

        public IList<Membresia> ObtenerActivos(
            int pagina = 1,
            int tamPagina = 10)
        {
            pagina = Math.Max(1, pagina);
            tamPagina = Math.Max(1, tamPagina);

            var lista = new List<Membresia>();

            using var conexion = CrearConexion();

            const string sql = @"
                SELECT
                    m.IdMembresia,
                    m.IdSocio,
                    m.IdPlan,
                    m.FechaInicio,
                    m.FechaFin,
                    m.PrecioContratado,
                    m.Estado,
                    m.Pagada,
                    m.IdUsuarioCreador,

                    s.IdSocio AS Socio_IdSocio,
                    s.Nombre AS Socio_Nombre,
                    s.Apellido AS Socio_Apellido,
                    s.Dni AS Socio_Dni,

                    p.IdPlan AS Plan_IdPlan,
                    p.Nombre AS Plan_Nombre,
                    p.Precio AS Plan_Precio,
                    p.UtilizacionesMensuales AS Plan_UtilizacionesMensuales

                FROM membresia m
                INNER JOIN socio s ON s.IdSocio = m.IdSocio
                INNER JOIN plan p ON p.IdPlan = m.IdPlan
                WHERE m.Estado = 1
                ORDER BY m.FechaInicio DESC, m.IdMembresia DESC
                LIMIT @TamPagina OFFSET @Desplazamiento;";

            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@TamPagina", tamPagina);
            comando.Parameters.AddWithValue(
                "@Desplazamiento", (pagina - 1) * tamPagina);

            conexion.Open();

            using var reader = comando.ExecuteReader();

            while (reader.Read())
                lista.Add(Mapear(reader));

            return lista;
        }

        public IList<Membresia> ObtenerInactivos(
            int pagina = 1,
            int tamPagina = 10)
        {
            pagina = Math.Max(1, pagina);
            tamPagina = Math.Max(1, tamPagina);

            var lista = new List<Membresia>();

            using var conexion = CrearConexion();

            const string sql = @"
                SELECT
                    m.IdMembresia,
                    m.IdSocio,
                    m.IdPlan,
                    m.FechaInicio,
                    m.FechaFin,
                    m.PrecioContratado,
                    m.Estado,
                    m.Pagada,
                    m.IdUsuarioCreador,

                    s.IdSocio AS Socio_IdSocio,
                    s.Nombre AS Socio_Nombre,
                    s.Apellido AS Socio_Apellido,
                    s.Dni AS Socio_Dni,

                    p.IdPlan AS Plan_IdPlan,
                    p.Nombre AS Plan_Nombre,
                    p.Precio AS Plan_Precio,
                    p.UtilizacionesMensuales AS Plan_UtilizacionesMensuales

                FROM membresia m
                INNER JOIN socio s ON s.IdSocio = m.IdSocio
                INNER JOIN plan p ON p.IdPlan = m.IdPlan
                WHERE m.Estado = 0
                ORDER BY m.FechaInicio DESC, m.IdMembresia DESC
                LIMIT @TamPagina OFFSET @Desplazamiento;";

            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@TamPagina", tamPagina);
            comando.Parameters.AddWithValue(
                "@Desplazamiento", (pagina - 1) * tamPagina);

            conexion.Open();

            using var reader = comando.ExecuteReader();

            while (reader.Read())
                lista.Add(Mapear(reader));

            return lista;
        }

        public int ObtenerCantidad(bool? soloActivos = true)
        {
            using var conexion = CrearConexion();

            const string sql = @"
                SELECT COUNT(*)
                FROM membresia
                WHERE (@Estado IS NULL OR Estado = @Estado);";

            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue(
                "@Estado",
                soloActivos.HasValue
                    ? (object)(soloActivos.Value ? 1 : 0)
                    : DBNull.Value);

            conexion.Open();

            return Convert.ToInt32(comando.ExecuteScalar());
        }

        public bool ExistePorSocio(int idSocio)
        {
            using var conexion = CrearConexion();

            const string sql = @"
                SELECT COUNT(*)
                FROM membresia
                WHERE IdSocio = @IdSocio;";

            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@IdSocio", idSocio);

            conexion.Open();

            return Convert.ToInt32(comando.ExecuteScalar()) > 0;
        }

        public Membresia? ObtenerParaCobroPorDni(string dni)
        {
            using var conexion = CrearConexion();

            const string sql = @"
                SELECT
                    m.IdMembresia,
                    m.IdSocio,
                    m.IdPlan,
                    m.FechaInicio,
                    m.FechaFin,
                    m.PrecioContratado,
                    m.Estado,
                    m.Pagada,
                    m.IdUsuarioCreador,

                    s.IdSocio AS Socio_IdSocio,
                    s.Nombre AS Socio_Nombre,
                    s.Apellido AS Socio_Apellido,
                    s.Dni AS Socio_Dni,

                    p.IdPlan AS Plan_IdPlan,
                    p.Nombre AS Plan_Nombre,
                    p.Precio AS Plan_Precio,
                    p.UtilizacionesMensuales AS Plan_UtilizacionesMensuales

                FROM membresia m
                INNER JOIN socio s ON s.IdSocio = m.IdSocio
                INNER JOIN plan p ON p.IdPlan = m.IdPlan
                WHERE s.Dni = @Dni
                  AND s.Estado = 1
                  AND m.Estado = 1
                ORDER BY m.FechaInicio DESC, m.IdMembresia DESC
                LIMIT 1;";

            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@Dni", dni.Trim());

            conexion.Open();

            using var reader = comando.ExecuteReader();

            return reader.Read() ? Mapear(reader) : null;
        }

        public IList<Membresia> ObtenerActivosPorSocio(
            int idSocio,
            int pagina = 1,
            int tamPagina = 10)
        {
            pagina = Math.Max(1, pagina);
            tamPagina = Math.Max(1, tamPagina);

            var lista = new List<Membresia>();

            using var conexion = CrearConexion();

            const string sql = @"
                SELECT
                    m.IdMembresia,
                    m.IdSocio,
                    m.IdPlan,
                    m.FechaInicio,
                    m.FechaFin,
                    m.PrecioContratado,
                    m.Estado,
                    m.Pagada,
                    m.IdUsuarioCreador,

                    s.IdSocio AS Socio_IdSocio,
                    s.Nombre AS Socio_Nombre,
                    s.Apellido AS Socio_Apellido,
                    s.Dni AS Socio_Dni,

                    p.IdPlan AS Plan_IdPlan,
                    p.Nombre AS Plan_Nombre,
                    p.Precio AS Plan_Precio,
                    p.UtilizacionesMensuales AS Plan_UtilizacionesMensuales

                FROM membresia m
                INNER JOIN socio s ON s.IdSocio = m.IdSocio
                INNER JOIN plan p ON p.IdPlan = m.IdPlan
                WHERE m.Estado = 1
                  AND m.IdSocio = @IdSocio
                ORDER BY m.FechaInicio DESC, m.IdMembresia DESC
                LIMIT @TamPagina OFFSET @Desplazamiento;";

            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@IdSocio", idSocio);
            comando.Parameters.AddWithValue("@TamPagina", tamPagina);
            comando.Parameters.AddWithValue(
                "@Desplazamiento", (pagina - 1) * tamPagina);

            conexion.Open();

            using var reader = comando.ExecuteReader();

            while (reader.Read())
                lista.Add(Mapear(reader));

            return lista;
        }

        private static Membresia Mapear(MySqlDataReader reader)
        {
            return new Membresia
            {
                IdMembresia = reader.GetInt32("IdMembresia"),
                IdSocio = reader.GetInt32("IdSocio"),
                IdPlan = reader.GetInt32("IdPlan"),
                FechaInicio = reader.GetDateTime("FechaInicio"),
                FechaFin = reader.GetDateTime("FechaFin"),
                PrecioContratado = reader.GetDecimal("PrecioContratado"),
                Estado = reader.GetBoolean("Estado"),
                Pagada = reader.GetBoolean("Pagada"),
                IdUsuarioCreador = reader.GetInt32("IdUsuarioCreador"),

                Socio = new Socio
                {
                    IdSocio = reader.GetInt32("Socio_IdSocio"),
                    Nombre = reader.GetString("Socio_Nombre"),
                    Apellido = reader.GetString("Socio_Apellido"),
                    Dni = reader.GetString("Socio_Dni")
                },

                Plan = new Plan
                {
                    IdPlan = reader.GetInt32("Plan_IdPlan"),
                    Nombre = reader.GetString("Plan_Nombre"),
                    Precio = reader.GetDecimal("Plan_Precio"),
                    UtilizacionesMensuales =
                        reader.IsDBNull(
                            reader.GetOrdinal("Plan_UtilizacionesMensuales"))
                            ? null
                            : reader.GetInt32("Plan_UtilizacionesMensuales")
                }
            };
        }

        private static void CargarParametros(
            MySqlCommand comando,
            Membresia membresia)
        {
            comando.Parameters.AddWithValue("@IdSocio", membresia.IdSocio);
            comando.Parameters.AddWithValue("@IdPlan", membresia.IdPlan);
            comando.Parameters.AddWithValue("@FechaInicio", membresia.FechaInicio);
            comando.Parameters.AddWithValue("@FechaFin", membresia.FechaFin);
            comando.Parameters.AddWithValue(
                "@PrecioContratado", membresia.PrecioContratado);
            comando.Parameters.AddWithValue("@Estado", membresia.Estado);
            comando.Parameters.AddWithValue("@Pagada", membresia.Pagada);
            comando.Parameters.AddWithValue(
                "@IdUsuarioCreador", membresia.IdUsuarioCreador);
        }
    }
}