using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using MySql.Data.MySqlClient;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
    public class RepositorioSocio : RepositorioBase, IRepositorioSocio
    {
        public RepositorioSocio(IConfiguration configuration) : base(configuration) { }


        public int Alta(Socio socio)
        {
            int res = -1;
            using var conexion = CrearConexion();

            var sql = @"INSERT INTO socio (Nombre, Apellido, Dni, Telefono, Domicilio, Avatar, Estado)
                    VALUES (@Nombre, @Apellido, @Dni, @Telefono, @Domicilio, @Avatar, @Estado);
                    SELECT LAST_INSERT_ID();";
            using var comand = new MySqlCommand(sql, conexion);

            CargarParametros(comand, socio);
            conexion.Open();
            res = Convert.ToInt32(comand.ExecuteScalar());
            socio.IdSocio = res;


            return res;
        }

        public int Modificacion(Socio socio)
        {
            int res = -1;
            using var conexion = CrearConexion();

            var sql = @"UPDATE socio SET Nombre=@Nombre, Apellido=@Apellido, Dni=@Dni, Telefono=@Telefono,
                           Domicilio=@Domicilio, Avatar=@Avatar
                    WHERE IdSocio=@IdSocio";
            using var comand = new MySqlCommand(sql, conexion);

            CargarParametros(comand, socio);
            conexion.Open();

            comand.Parameters.AddWithValue("@IdSocio", socio.IdSocio);
            res = comand.ExecuteNonQuery();

            return res;
        }

        // Baja lógica
        public int Baja(int id)
        {
            int res = -1;
            using var conexion = CrearConexion();
            conexion.Open();

            string sql = @"UPDATE socio SET Estado=0 WHERE IdSocio=@id";


            using var comand = new MySqlCommand(sql, conexion);
            comand.Parameters.AddWithValue("@id", id);
            res = comand.ExecuteNonQuery();

            return res;
        }
        public int Reactivar(int id)
        {
            int filas = 0;

            using (var conexion = CrearConexion())
            {
                string sql = @"
            UPDATE socio
            SET Estado = 1
            WHERE IdSocio = @id AND Estado = 0;";

                using (var comand = new MySqlCommand(sql, conexion))
                {
                    comand.Parameters.AddWithValue("@id", id);
                    conexion.Open();
                    filas = comand.ExecuteNonQuery();
                }
            }

            return filas;
        }

        public Socio? ObtenerPorId(int IdSocio)
        {
            Socio? socio = null;
            using var conexion = CrearConexion();

            string sql = @"SELECT IdSocio, Nombre, Apellido, Dni, Telefono, Domicilio, Avatar, Estado FROM socio WHERE IdSocio=@IdSocio";

            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue("@IdSocio", IdSocio);
            conexion.Open();
            using var read = comand.ExecuteReader();

            socio = read.Read() ? Mapear(read) : null;



            return socio;
        }

        public Socio? ObtenerPorDni(string dni)
        {
            Socio? socio = null;
            using var conexion = CrearConexion();

            string sql = @"SELECT IdSocio, Nombre, Apellido, Dni, Telefono, Domicilio, Avatar, Estado FROM socio WHERE Dni=@dni";
            using var comand = new MySqlCommand(sql, conexion);
            comand.Parameters.AddWithValue("@dni", dni);
            conexion.Open();
            using var read = comand.ExecuteReader();

            socio = read.Read() ? Mapear(read) : null;

            return socio;
        }

        public bool ExisteDni(string dni, int? excluirId = null)
        {
            bool res = false;
            using var conexion = CrearConexion();

            string sql = @"SELECT COUNT(*) FROM socio WHERE Dni=@Dni AND (@excluir IS NULL OR IdSocio<>@excluir)";
            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue("@Dni", dni);

            comand.Parameters.AddWithValue("@excluir", ValorODbNull(excluirId));
            conexion.Open();
            res = Convert.ToInt32(comand.ExecuteScalar()) > 0;
            return res;
        }

        // Listado de socios, filtrando opcionalmente por estado (null = todos)
        public IList<Socio> ObtenerActivos(int pagina = 1, int tamPagina = 10)
        {
            if (pagina < 1) pagina = 1;
            if (tamPagina < 1) tamPagina = 10;

            IList<Socio> lista = new List<Socio>();

            using (var conexion = CrearConexion())
            {
                string sql = @"
            SELECT IdSocio, Nombre, Apellido, Dni, Telefono, Domicilio, Avatar, Estado
            FROM socio
            WHERE Estado = 1
            ORDER BY Apellido, Nombre, IdSocio
            LIMIT @tamPagina OFFSET @desplazamiento;";

                using (var comand = new MySqlCommand(sql, conexion))
                {
                    comand.Parameters.AddWithValue("@tamPagina", tamPagina);
                    comand.Parameters.AddWithValue("@desplazamiento", (pagina - 1) * tamPagina);
                    conexion.Open();

                    using (var read = comand.ExecuteReader())
                    {
                        while (read.Read())
                        {
                            lista.Add(Mapear(read));
                        }
                    }
                }
            }

            return lista;
        }

        public IList<Socio> ObtenerInactivos(int pagina = 1, int tamPagina = 10)
        {
            if (pagina < 1) pagina = 1;
            if (tamPagina < 1) tamPagina = 10;

            IList<Socio> lista = new List<Socio>();

            using (var conexion = CrearConexion())
            {
                string sql = @"
            SELECT IdSocio, Nombre, Apellido, Dni, Telefono, Domicilio, Avatar, Estado
            FROM socio
            WHERE Estado = 0
            ORDER BY Apellido, Nombre, IdSocio
            LIMIT @tamPagina OFFSET @desplazamiento;";

                using (var comand = new MySqlCommand(sql, conexion))
                {
                    comand.Parameters.AddWithValue("@tamPagina", tamPagina);
                    comand.Parameters.AddWithValue("@desplazamiento", (pagina - 1) * tamPagina);
                    conexion.Open();

                    using (var read = comand.ExecuteReader())
                    {
                        while (read.Read())
                        {
                            lista.Add(Mapear(read));
                        }
                    }
                }
            }

            return lista;
        }
        public int ObtenerCantidad(bool? soloActivos = true)
        {
            int cantidad = 0;

            using (var conexion = CrearConexion())
            {
                string sql = @"
            SELECT COUNT(*)
            FROM socio
            WHERE (@estado IS NULL OR Estado = @estado);";

                using (var comand = new MySqlCommand(sql, conexion))
                {
                    comand.Parameters.AddWithValue("@estado", ValorODbNull(soloActivos));
                    conexion.Open();
                    cantidad = Convert.ToInt32(comand.ExecuteScalar());
                }
            }

            return cantidad;
        }


        private static void CargarParametros(MySqlCommand cmd, Socio s)
        {
            cmd.Parameters.AddWithValue("@Nombre", s.Nombre ?? string.Empty);
            cmd.Parameters.AddWithValue("@Apellido", s.Apellido ?? string.Empty);
            cmd.Parameters.AddWithValue("@Dni", s.Dni ?? string.Empty);
            cmd.Parameters.AddWithValue("@Telefono", s.Telefono ?? string.Empty);
            cmd.Parameters.AddWithValue("@Domicilio", s.Domicilio ?? string.Empty);
            cmd.Parameters.AddWithValue("@Avatar", s.Avatar ?? string.Empty);
            cmd.Parameters.AddWithValue("@Estado", s.Estado);
        }

        private static Socio Mapear(MySqlDataReader r) => new Socio
        {
            IdSocio = r.GetInt32("IdSocio"),
            Nombre = r.GetString("Nombre"),
            Apellido = r.GetString("Apellido"),
            Dni = r.GetString("Dni"),
            Telefono = r.GetString("Telefono"),
            Domicilio = r.GetString("Domicilio"),
            Avatar = r.GetString("Avatar"),
            Estado = r.GetBoolean("Estado")
        };
    }
}