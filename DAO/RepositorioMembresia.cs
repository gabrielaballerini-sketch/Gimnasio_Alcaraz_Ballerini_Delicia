using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using MySql.Data.MySqlClient;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
    public class RepositorioMembresia : RepositorioBase, IRepositorioMembresia
    {
        public RepositorioMembresia(IConfiguration configuration) : base(configuration)
        {
        }

        public int Alta(Membresia membresia)
        {
            using var conexion = CrearConexion();

            var sql = @"
                INSERT INTO membresia
                    (
                        IdSocio,
                        IdPlan,
                        FechaInicio,
                        FechaFin,
                        PrecioContratado,
                        Estado,
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
                        @IdUsuarioCreador
                    );

                SELECT LAST_INSERT_ID();";

            using var comand =
                new MySqlCommand(sql, conexion);

            CargarParametros(comand, membresia);

            conexion.Open();

            var id = Convert.ToInt32(
                comand.ExecuteScalar()
            );

            membresia.IdMembresia = id;

            return id;
        }

        public int Modificacion(Membresia membresia)
        {
            using var conexion = CrearConexion();

            var sql = @"
                UPDATE membresia
                SET IdSocio = @IdSocio,
                    IdPlan = @IdPlan,
                    FechaInicio = @FechaInicio,
                    FechaFin = @FechaFin,
                    PrecioContratado = @PrecioContratado
                WHERE IdMembresia = @IdMembresia;";

            using var comand =
                new MySqlCommand(sql, conexion);

            CargarParametros(comand, membresia);

            comand.Parameters.AddWithValue("@Idmembresia", membresia.IdMembresia);

            conexion.Open();

            return comand.ExecuteNonQuery();
        }

        public int Baja(int id)
        {
            using var conexion = CrearConexion();

            var sql = @"
                UPDATE membresia
                SET Estado = 0
                WHERE IdMembresia = @IdMembresia;";

            using var comand =
                new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue(
                "@IdMembresia",
                id
            );

            conexion.Open();

            return comand.ExecuteNonQuery();
        }

        public int Reactivar(int id)
        {
            using var conexion = CrearConexion();

            var sql = @"
                UPDATE membresia
                SET Estado = 1
                WHERE IdMembresia = @IdMembresia
                  AND Estado = 0;";

            using var comand =
                new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue(
                "@IdMembresia",
                id
            );

            conexion.Open();

            return comand.ExecuteNonQuery();
        }

        public Membresia? ObtenerPorId(int id)
        {
            using var conexion = CrearConexion();

            var sql = @"
                SELECT
                    m.IdMembresia,
                    m.IdSocio,
                    m.IdPlan,
                    m.FechaInicio,
                    m.FechaFin,
                    m.PrecioContratado,
                    m.Estado,
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

                INNER JOIN socio s
                    ON s.IdSocio = m.IdSocio

                INNER JOIN plan p
                    ON p.IdPlan = m.IdPlan

                WHERE m.IdMembresia = @IdMembresia;";

            using var comand =
                new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue(
                "@IdMembresia",
                id
            );

            conexion.Open();

            using var read = comand.ExecuteReader();

            return read.Read() ? Mapear(read) : null;
        }

        public IList<Membresia> ObtenerActivos(
            int pagina = 1,
            int tamPagina = 10)
        {
            if (pagina < 1)
                pagina = 1;

            if (tamPagina < 1)
                tamPagina = 10;

            IList<Membresia> lista =
                new List<Membresia>();

            using var conexion = CrearConexion();

            var sql = @"
                SELECT
                    m.IdMembresia,
                    m.IdSocio,
                    m.IdPlan,
                    m.FechaInicio,
                    m.FechaFin,
                    m.PrecioContratado,
                    m.Estado,
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

                INNER JOIN socio s
                    ON s.IdSocio = m.IdSocio

                INNER JOIN plan p
                    ON p.IdPlan = m.IdPlan

                WHERE m.Estado = 1

                ORDER BY m.FechaInicio DESC, m.IdMembresia DESC

                LIMIT @tamPagina
                OFFSET @desplazamiento;";

            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue(
                "@tamPagina",
                tamPagina
            );

            comand.Parameters.AddWithValue(
                "@desplazamiento",
                (pagina - 1) * tamPagina
            );

            conexion.Open();

            using var read =
                comand.ExecuteReader();

            while (read.Read())
            {
                lista.Add(
                    Mapear(read)
                );
            }

            return lista;
        }

        public IList<Membresia> ObtenerInactivos(
            int pagina = 1,
            int tamPagina = 10)
        {
            if (pagina < 1)
                pagina = 1;

            if (tamPagina < 1)
                tamPagina = 10;

            IList<Membresia> lista =
                new List<Membresia>();

            using var conexion = CrearConexion();

            var sql = @"
                SELECT
                    m.IdMembresia,
                    m.IdSocio,
                    m.IdPlan,
                    m.FechaInicio,
                    m.FechaFin,
                    m.PrecioContratado,
                    m.Estado,
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

                INNER JOIN socio s
                    ON s.IdSocio = m.IdSocio

                INNER JOIN plan p
                    ON p.IdPlan = m.IdPlan

                WHERE m.Estado = 0

                ORDER BY m.FechaInicio DESC, m.IdMembresia DESC

                LIMIT @tamPagina
                OFFSET @desplazamiento;";

            using var comand =
                new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue(
                "@tamPagina",
                tamPagina
            );

            comand.Parameters.AddWithValue(
                "@desplazamiento",
                (pagina - 1) * tamPagina
            );

            conexion.Open();

            using var read =
                comand.ExecuteReader();

            while (read.Read())
            {
                lista.Add(
                    Mapear(read)
                );
            }

            return lista;
        }

        public int ObtenerCantidad(
            bool? soloActivos = true)
        {
            using var conexion = CrearConexion();

            var sql = @"
                SELECT COUNT(*)
                FROM membresia
                WHERE
                    (@Estado IS NULL
                    OR Estado = @Estado);";

            using var comand =
                new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue(
                "@Estado",
                ValorODbNull(soloActivos)
            );

            conexion.Open();

            return Convert.ToInt32(
                comand.ExecuteScalar()
            );
        }

        public IList<Membresia> ObtenerActivosPorSocio(
            int idSocio,
            int pagina = 1,
            int tamPagina = 10)
        {
            if (pagina < 1)
                pagina = 1;

            if (tamPagina < 1)
                tamPagina = 10;

            IList<Membresia> lista =
                new List<Membresia>();

            using var conexion = CrearConexion();

            var sql = @"
                SELECT
                    m.IdMembresia,
                    m.IdSocio,
                    m.IdPlan,
                    m.FechaInicio,
                    m.FechaFin,
                    m.PrecioContratado,
                    m.Estado,
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

                INNER JOIN socio s
                    ON s.IdSocio = m.IdSocio

                INNER JOIN plan p
                    ON p.IdPlan = m.IdPlan

                WHERE m.Estado = 1
                  AND m.IdSocio = @IdSocio

                ORDER BY m.FechaInicio DESC, m.IdMembresia DESC

                LIMIT @tamPagina
                OFFSET @desplazamiento;";

            using var comand =
                new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue(
                "@IdSocio",
                idSocio
            );

            comand.Parameters.AddWithValue(
                "@tamPagina",
                tamPagina
            );

            comand.Parameters.AddWithValue(
                "@desplazamiento",
                (pagina - 1) * tamPagina
            );

            conexion.Open();

            using var read =
                comand.ExecuteReader();

            while (read.Read())
            {
                lista.Add(
                    Mapear(read)
                );
            }

            return lista;
        }

        private static Membresia Mapear(
            MySqlDataReader r)
        {
            return new Membresia
            {
                IdMembresia =
                    r.GetInt32("IdMembresia"),

                IdSocio =
                    r.GetInt32("IdSocio"),

                IdPlan =
                    r.GetInt32("IdPlan"),

                FechaInicio =
                    r.GetDateTime("FechaInicio"),

                FechaFin =
                    r.GetDateTime("FechaFin"),

                PrecioContratado =
                    r.GetDecimal("PrecioContratado"),

                Estado =
                    r.GetBoolean("Estado"),

                IdUsuarioCreador =
                    r.GetInt32("IdUsuarioCreador"),

                Socio = new Socio
                {
                    IdSocio =
                        r.GetInt32("Socio_IdSocio"),

                    Nombre =
                        r.GetString("Socio_Nombre"),

                    Apellido =
                        r.GetString("Socio_Apellido"),

                    Dni =
                        r.GetString("Socio_Dni")
                },

                Plan = new Plan
                {
                    IdPlan =
                        r.GetInt32("Plan_IdPlan"),

                    Nombre =
                        r.GetString("Plan_Nombre"),

                    Precio =
                        r.GetDecimal("Plan_Precio"),

                    UtilizacionesMensuales =
                        r.IsDBNull(
                            r.GetOrdinal(
                                "Plan_UtilizacionesMensuales"
                            )
                        )
                            ? null
                            : r.GetInt32(
                                "Plan_UtilizacionesMensuales"
                            )
                }
            };
        }

        private static void CargarParametros(MySqlCommand cmd, Membresia membresia)
        {
            cmd.Parameters.AddWithValue(
                "@IdSocio",
                membresia.IdSocio
            );

            cmd.Parameters.AddWithValue(
                "@IdPlan",
                membresia.IdPlan
            );

            cmd.Parameters.AddWithValue(
                "@FechaInicio",
                membresia.FechaInicio
            );

            cmd.Parameters.AddWithValue(
                "@FechaFin",
                membresia.FechaFin
            );

            cmd.Parameters.AddWithValue(
                "@PrecioContratado",
                membresia.PrecioContratado
            );

            cmd.Parameters.AddWithValue(
                "@Estado",
                membresia.Estado
            );

            cmd.Parameters.AddWithValue(
                "@IdUsuarioCreador",
                membresia.IdUsuarioCreador
            );
        }
    }
}