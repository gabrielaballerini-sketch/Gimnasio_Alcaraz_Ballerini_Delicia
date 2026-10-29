using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Gimnasio_Alcaraz_Ballerini_Delicia.Service;
using MySql.Data.MySqlClient;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
    public class RepositorioActividad : RepositorioBase, IRepositorioActividad
    {
        public RepositorioActividad(IConfiguration configuration)
            : base(configuration)
        {
        }

        public int Alta(Actividad actividad)
        {
            using var conexion = CrearConexion();

            var sql = @"
            INSERT INTO actividad
                (Nombre, Estado, AccesoLibre)
            VALUES
                (@Nombre, @Estado, @AccesoLibre);

            SELECT LAST_INSERT_ID();";

            using var comand = new MySqlCommand(sql, conexion);

            CargarParametros(comand, actividad);

            conexion.Open();

            var id = Convert.ToInt32(
                comand.ExecuteScalar()
            );

            actividad.IdActividad = id;

            return id;
        }


        public int Modificacion(Actividad actividad)
        {
            using var conexion = CrearConexion();

            var sql = @"
            UPDATE actividad
            SET Nombre = @Nombre,
                AccesoLibre = @AccesoLibre
            WHERE IdActividad = @IdActividad;";

            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue(
                "@Nombre",
                actividad.Nombre ?? string.Empty
            );

            comand.Parameters.AddWithValue(
                "@AccesoLibre",
                actividad.AccesoLibre
            );

            comand.Parameters.AddWithValue(
                "@IdActividad",
                actividad.IdActividad
            );

            conexion.Open();

            return comand.ExecuteNonQuery();
        }


        public int Baja(int id)
        {
            using var conexion = CrearConexion();

            var sql = @"
            UPDATE actividad
            SET Estado = 0
            WHERE IdActividad = @IdActividad;";

            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue(
                "@IdActividad",
                id
            );

            conexion.Open();

            return comand.ExecuteNonQuery();
        }


        public int Reactivar(int id)
        {
            using var conexion = CrearConexion();

            var sql = @"
            UPDATE actividad
            SET Estado = 1
            WHERE IdActividad = @IdActividad
              AND Estado = 0;";

            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue(
                "@IdActividad",
                id
            );

            conexion.Open();

            return comand.ExecuteNonQuery();
        }


        public Actividad? ObtenerPorId(int id)
        {
            using var conexion = CrearConexion();

            var sql = @"
            SELECT
                IdActividad,
                Nombre,
                Estado,
                AccesoLibre
            FROM actividad
            WHERE IdActividad = @IdActividad;";

            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue(
                "@IdActividad",
                id
            );

            conexion.Open();

            using var read = comand.ExecuteReader();

            return read.Read()
                ? Mapear(read)
                : null;
        }


        public IList<Actividad> ObtenerActivos(
            int pagina = 1,
            int tamPagina = 10)
        {
           (pagina, tamPagina) =PaginacionParametros.Normalizar(pagina, tamPagina);


            IList<Actividad> lista =
                new List<Actividad>();

            using var conexion = CrearConexion();

            var sql = @"
            SELECT
                IdActividad,
                Nombre,
                Estado,
                AccesoLibre
            FROM actividad
            WHERE Estado = 1
            ORDER BY Nombre, IdActividad
            LIMIT @tamPagina
            OFFSET @desplazamiento;";

            using var comand = new MySqlCommand(
                sql,
                conexion
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


        public IList<Actividad> ObtenerInactivos(
            int pagina = 1,
            int tamPagina = 10)
        {
         (pagina, tamPagina) =PaginacionParametros.Normalizar(pagina, tamPagina);


            IList<Actividad> lista =
                new List<Actividad>();

            using var conexion = CrearConexion();

            var sql = @"
            SELECT
                IdActividad,
                Nombre,
                Estado,
                AccesoLibre
            FROM actividad
            WHERE Estado = 0
            ORDER BY Nombre, IdActividad
            LIMIT @tamPagina
            OFFSET @desplazamiento;";

            using var comand = new MySqlCommand(
                sql,
                conexion
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


        public int ObtenerCantidad(
            bool? soloActivos = true)
        {
            using var conexion = CrearConexion();

            var sql = @"
            SELECT COUNT(*)
            FROM actividad
            WHERE
                (@Estado IS NULL
                OR Estado = @Estado);";

            using var comand = new MySqlCommand(
                sql,
                conexion
            );

            comand.Parameters.AddWithValue(
                "@Estado",
                ValorODbNull(soloActivos)
            );

            conexion.Open();

            return Convert.ToInt32(
                comand.ExecuteScalar()
            );
        }
        public IList<Actividad> Buscar(string? texto, bool soloProgramables, int limite = 10)
        {
            IList<Actividad> lista = new List<Actividad>();

            using var conexion = CrearConexion();

            var sql = @"
            SELECT IdActividad, Nombre, Estado, AccesoLibre
            FROM actividad
            WHERE Estado = 1
              AND (@soloProgramables = 0 OR AccesoLibre = 0)
              AND Nombre LIKE @patron
            ORDER BY Nombre, IdActividad
            LIMIT @limite;";

            using var comand = new MySqlCommand(sql, conexion);
            comand.Parameters.AddWithValue("@soloProgramables", soloProgramables ? 1 : 0);
            comand.Parameters.AddWithValue("@patron", PatronContiene(texto));
            comand.Parameters.AddWithValue("@limite", limite);

            conexion.Open();

            using var read = comand.ExecuteReader();
            while (read.Read())
                lista.Add(Mapear(read));

            return lista;
        }




        private static void CargarParametros(
            MySqlCommand cmd,
            Actividad actividad)
        {
            cmd.Parameters.AddWithValue(
                "@Nombre",
                actividad.Nombre ?? string.Empty
            );

            cmd.Parameters.AddWithValue(
                "@Estado",
                actividad.Estado
            );

            cmd.Parameters.AddWithValue(
                "@AccesoLibre",
                actividad.AccesoLibre
            );
        }


        private static Actividad Mapear(
            MySqlDataReader r)
        {
            return new Actividad
            {
                IdActividad =
                    r.GetInt32("IdActividad"),

                Nombre =
                    r.GetString("Nombre"),

                Estado =
                    r.GetBoolean("Estado"),

                AccesoLibre =
                    r.GetBoolean("AccesoLibre")
            };
        }
    }
}
