using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using MySql.Data.MySqlClient;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
    public class RepositorioSocio : RepositorioBase, IRepositorioSocio
    {
        public RepositorioSocio(IConfiguration configuration) : base(configuration) { }

        private const string Columnas =
            "IdSocio, Nombre, Apellido, Dni, Telefono, Domicilio, Estado, IdUsuario";

        // Alta de un socio solo, sin usuario
        public int Alta(Socio socio)
        {
            using var conexion = CrearConexion();

            var sql = @"INSERT INTO socio (Nombre, Apellido, Dni, Telefono, Domicilio, Estado)
                        VALUES (@Nombre, @Apellido, @Dni, @Telefono, @Domicilio, @Estado);
                        SELECT LAST_INSERT_ID();";

            using var comand = new MySqlCommand(sql, conexion);

            CargarParametros(comand, socio);
            conexion.Open();

            socio.IdSocio = Convert.ToInt32(comand.ExecuteScalar());

            return socio.IdSocio;
        }

        // Crea el usuario y el socio en una sola transacción: o se guardan los dos o ninguno
        public int AltaConUsuario(Socio socio, Usuario usuario)
        {
            using var conexion = CrearConexion();
            conexion.Open();

            using var transaccion = conexion.BeginTransaction();

            try
            {
                var sqlUsuario = @"
                    INSERT INTO usuario (Email, NombreUsuario, Password, Roles, Estado, Avatar)
                    VALUES (@Email, @NombreUsuario, @Password, @Roles, @Estado, NULL);
                    SELECT LAST_INSERT_ID();";

                using (var comandUsuario = new MySqlCommand(sqlUsuario, conexion, transaccion))
                {
                    comandUsuario.Parameters.AddWithValue("@Email", usuario.Email ?? string.Empty);
                    comandUsuario.Parameters.AddWithValue("@NombreUsuario", ValorODbNull(usuario.NombreUsuario));
                    comandUsuario.Parameters.AddWithValue("@Password", usuario.Password ?? string.Empty);
                    comandUsuario.Parameters.AddWithValue("@Roles", (int)usuario.Roles);
                    comandUsuario.Parameters.AddWithValue("@Estado", usuario.Estado);

                    usuario.IdUsuario = Convert.ToInt32(comandUsuario.ExecuteScalar());
                }

                socio.IdUsuario = usuario.IdUsuario;

                var sqlSocio = @"
                    INSERT INTO socio (Nombre, Apellido, Dni, Telefono, Domicilio, Estado, IdUsuario)
                    VALUES (@Nombre, @Apellido, @Dni, @Telefono, @Domicilio, @Estado, @IdUsuario);
                    SELECT LAST_INSERT_ID();";

                using (var comandSocio = new MySqlCommand(sqlSocio, conexion, transaccion))
                {
                    CargarParametros(comandSocio, socio);
                    comandSocio.Parameters.AddWithValue("@IdUsuario", usuario.IdUsuario);

                    socio.IdSocio = Convert.ToInt32(comandSocio.ExecuteScalar());
                }

                transaccion.Commit();

                return socio.IdSocio;
            }
            catch
            {
                transaccion.Rollback();
                throw;
            }
        }

        // No modifica Estado ni IdUsuario
        public int Modificacion(Socio socio)
        {
            using var conexion = CrearConexion();

            var sql = @"UPDATE socio
                        SET Nombre = @Nombre, Apellido = @Apellido, Dni = @Dni,
                            Telefono = @Telefono, Domicilio = @Domicilio
                        WHERE IdSocio = @IdSocio";

            using var comand = new MySqlCommand(sql, conexion);

            CargarParametros(comand, socio);
            comand.Parameters.AddWithValue("@IdSocio", socio.IdSocio);
            conexion.Open();

            return comand.ExecuteNonQuery();
        }

        // Baja lógica
        public int Baja(int id)
        {
            using var conexion = CrearConexion();

            using var comand = new MySqlCommand(
                "UPDATE socio SET Estado = 0 WHERE IdSocio = @id", conexion);

            comand.Parameters.AddWithValue("@id", id);
            conexion.Open();

            return comand.ExecuteNonQuery();
        }

        public int Reactivar(int id)
        {
            using var conexion = CrearConexion();

            using var comand = new MySqlCommand(
                "UPDATE socio SET Estado = 1 WHERE IdSocio = @id AND Estado = 0;", conexion);

            comand.Parameters.AddWithValue("@id", id);
            conexion.Open();

            return comand.ExecuteNonQuery();
        }

        public Socio? ObtenerPorId(int idSocio)
        {
            using var conexion = CrearConexion();

            using var comand = new MySqlCommand(
                $"SELECT {Columnas} FROM socio WHERE IdSocio = @IdSocio", conexion);

            comand.Parameters.AddWithValue("@IdSocio", idSocio);
            conexion.Open();

            using var read = comand.ExecuteReader();

            return read.Read() ? Mapear(read) : null;
        }

        public Socio? ObtenerPorDni(string dni)
        {
            using var conexion = CrearConexion();

            using var comand = new MySqlCommand(
                $"SELECT {Columnas} FROM socio WHERE Dni = @dni", conexion);

            comand.Parameters.AddWithValue("@dni", dni);
            conexion.Open();

            using var read = comand.ExecuteReader();

            return read.Read() ? Mapear(read) : null;
        }

        public Socio? ObtenerPorIdUsuario(int idUsuario)
        {
            using var conexion = CrearConexion();

            using var comand = new MySqlCommand(
                $"SELECT {Columnas} FROM socio WHERE IdUsuario = @idUsuario", conexion);

            comand.Parameters.AddWithValue("@idUsuario", idUsuario);
            conexion.Open();

            using var read = comand.ExecuteReader();

            return read.Read() ? Mapear(read) : null;
        }

        public bool ExisteDni(string dni, int? excluirId = null)
        {
            using var conexion = CrearConexion();

            string sql = @"SELECT COUNT(*) FROM socio
                           WHERE Dni = @Dni AND (@excluir IS NULL OR IdSocio <> @excluir)";

            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue("@Dni", dni);
            comand.Parameters.AddWithValue("@excluir", ValorODbNull(excluirId));
            conexion.Open();

            return Convert.ToInt32(comand.ExecuteScalar()) > 0;
        }

        public IList<Socio> ObtenerActivos(int pagina = 1, int tamPagina = 10)
        {
            if (pagina < 1) pagina = 1;
            if (tamPagina < 1) tamPagina = 10;

            IList<Socio> lista = new List<Socio>();

            using (var conexion = CrearConexion())
            {
                string sql = $@"
                    SELECT {Columnas}
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
                string sql = $@"
                    SELECT {Columnas}
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

        public IList<Socio> Buscar(string texto, int limite = 10, bool soloSinUsuario = false)
        {
            IList<Socio> lista = new List<Socio>();

            // Se escapan % y _ para que se busquen como caracteres comunes
            string patron = "%" + texto
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_") + "%";

            using (var conexion = CrearConexion())
            {
                string sql = $@"
                    SELECT {Columnas}
                    FROM socio
                    WHERE Estado = 1
                      AND (Nombre LIKE @patron
                           OR Apellido LIKE @patron
                           OR Dni LIKE @patron
                           OR CONCAT(Apellido, ' ', Nombre) LIKE @patron
                           OR CONCAT(Nombre, ' ', Apellido) LIKE @patron)
                      AND (@soloSinUsuario = 0 OR IdUsuario IS NULL)
                    ORDER BY Apellido, Nombre, IdSocio
                    LIMIT @limite;";

                using (var comand = new MySqlCommand(sql, conexion))
                {
                    comand.Parameters.AddWithValue("@patron", patron);
                    comand.Parameters.AddWithValue("@soloSinUsuario", soloSinUsuario);
                    comand.Parameters.AddWithValue("@limite", limite);
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

        // Devuelve 0 si el socio ya tiene OTRO usuario (nunca pisa un vínculo ajeno)
        public int VincularUsuario(int idSocio, int idUsuario)
        {
            using var conexion = CrearConexion();

            string sql = @"
                UPDATE socio
                SET IdUsuario = @idUsuario
                WHERE IdSocio = @idSocio
                  AND (IdUsuario IS NULL OR IdUsuario = @idUsuario);";

            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue("@idUsuario", idUsuario);
            comand.Parameters.AddWithValue("@idSocio", idSocio);
            conexion.Open();

            return comand.ExecuteNonQuery();
        }

        // Libera el socio que tuviera ese usuario
        public int DesvincularUsuario(int idUsuario)
        {
            using var conexion = CrearConexion();

            using var comand = new MySqlCommand(
                "UPDATE socio SET IdUsuario = NULL WHERE IdUsuario = @idUsuario;", conexion);

            comand.Parameters.AddWithValue("@idUsuario", idUsuario);
            conexion.Open();

            return comand.ExecuteNonQuery();
        }

        private static void CargarParametros(MySqlCommand cmd, Socio s)
        {
            cmd.Parameters.AddWithValue("@Nombre", s.Nombre ?? string.Empty);
            cmd.Parameters.AddWithValue("@Apellido", s.Apellido ?? string.Empty);
            cmd.Parameters.AddWithValue("@Dni", s.Dni ?? string.Empty);
            cmd.Parameters.AddWithValue("@Telefono", s.Telefono ?? string.Empty);
            cmd.Parameters.AddWithValue("@Domicilio", s.Domicilio ?? string.Empty);
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
            Estado = r.GetBoolean("Estado"),
            IdUsuario = r.IsDBNull(r.GetOrdinal("IdUsuario"))
                ? null
                : r.GetInt32("IdUsuario")
        };
    }
}