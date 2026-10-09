
using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using MySql.Data.MySqlClient;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
    public class RepositorioUsuario
        : RepositorioBase, IRepositorioUsuario
    {
        public RepositorioUsuario(
            IConfiguration configuration)
            : base(configuration)
        {
        }

        public int Alta(Usuario usuario)
        {
            using var conexion = CrearConexion();

            var sql = @"
                INSERT INTO usuario
                    (Email,NombreUsuario, Password, Roles, Estado, Avatar)
                VALUES
                    (@Email,@NombreUsuario, @Password, @Roles, @Estado, @Avatar);

                SELECT LAST_INSERT_ID();";

            using var comando = new MySqlCommand(sql, conexion);

            CargarParametros(comando, usuario);

            conexion.Open();

            int id = Convert.ToInt32(
                comando.ExecuteScalar()
            );

            usuario.IdUsuario = id;

            return id;
        }

        public int Modificacion(Usuario usuario)
        {
            using var conexion = CrearConexion();

            var sql = @"
                UPDATE usuario
                SET Email = @Email,
                    NombreUsuario=@NombreUsuario,
                    Password = @Password,
                    Roles = @Roles,
                    Estado = @Estado,
                    Avatar = @Avatar
                WHERE IdUsuario = @IdUsuario;";

            using var comando = new MySqlCommand(sql, conexion);

            CargarParametros(comando, usuario);

            comando.Parameters.AddWithValue(
                "@IdUsuario",
                usuario.IdUsuario
            );

            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        public int Baja(int id)
        {
            using var conexion = CrearConexion();

            var sql = @"
                UPDATE usuario
                SET Estado = 0
                WHERE IdUsuario = @id;";

            using var comando = new MySqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("@id", id);

            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        public int Reactivar(int id)
        {
            using var conexion = CrearConexion();

            var sql = @"
                UPDATE usuario
                SET Estado = 1
                WHERE IdUsuario = @id
                  AND Estado = 0;";

            using var comando = new MySqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("@id", id);

            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        public Usuario? ObtenerPorId(int id)
        {
            using var conexion = CrearConexion();

            var sql = @"
                SELECT IdUsuario, Email, NombreUsuario, Password, Roles,
                       Estado, Avatar
                FROM usuario
                WHERE IdUsuario = @IdUsuario;";

            using var comando = new MySqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("@IdUsuario", id);

            conexion.Open();

            using var reader = comando.ExecuteReader();

            return reader.Read() ? Mapear(reader) : null;
        }

        public Usuario? ObtenerPorEmail(string email)
        {
            using var conexion = CrearConexion();

            var sql = @"
                SELECT IdUsuario, Email, NombreUsuario, Password, Roles,
                       Estado, Avatar
                FROM usuario
                WHERE Email = @Email;";

            using var comando = new MySqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("@Email", email);

            conexion.Open();

            using var reader = comando.ExecuteReader();

            return reader.Read() ? Mapear(reader) : null;
        }

        public bool ExisteEmail(
            string email,
            int? excluirId = null)
        {
            using var conexion = CrearConexion();

            var sql = @"
                SELECT COUNT(*)
                FROM usuario
                WHERE Email = @Email
                  AND (@ExcluirId IS NULL
                       OR IdUsuario <> @ExcluirId);";

            using var comando = new MySqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("@Email", email);

            comando.Parameters.AddWithValue(
                "@ExcluirId",
                ValorODbNull(excluirId)
            );

            conexion.Open();

            return Convert.ToInt32(
                comando.ExecuteScalar()
            ) > 0;
        }

        public IList<Usuario> ObtenerActivos(
            int pagina = 1,
            int tamPagina = 10)
        {
            return ObtenerPorEstado(true, pagina, tamPagina);
        }

        public IList<Usuario> ObtenerInactivos(
            int pagina = 1,
            int tamPagina = 10)
        {
            return ObtenerPorEstado(false, pagina, tamPagina);
        }

        private IList<Usuario> ObtenerPorEstado(
            bool estado,
            int pagina,
            int tamPagina)
        {
            if (pagina < 1)
                pagina = 1;

            if (tamPagina < 1)
                tamPagina = 10;

            IList<Usuario> lista = new List<Usuario>();

            using var conexion = CrearConexion();

            var sql = @"
                SELECT IdUsuario, Email,NombreUsuario, Password, Roles,
                       Estado, Avatar
                FROM usuario
                WHERE Estado = @Estado
                ORDER BY Email, IdUsuario
                LIMIT @TamPagina
                OFFSET @Desplazamiento;";

            using var comando = new MySqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("@Estado", estado);
            comando.Parameters.AddWithValue("@TamPagina", tamPagina);
            comando.Parameters.AddWithValue(
                "@Desplazamiento",
                (pagina - 1) * tamPagina
            );

            conexion.Open();

            using var reader = comando.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(Mapear(reader));
            }

            return lista;
        }

        public int ObtenerCantidad(bool? soloActivos = true)
        {
            using var conexion = CrearConexion();

            var sql = @"
                SELECT COUNT(*)
                FROM usuario
                WHERE (@Estado IS NULL OR Estado = @Estado);";

            using var comando = new MySqlCommand(sql, conexion);

            comando.Parameters.AddWithValue(
                "@Estado",
                ValorODbNull(soloActivos)
            );

            conexion.Open();

            return Convert.ToInt32(comando.ExecuteScalar());
        }

        private static void CargarParametros(
            MySqlCommand comando,
            Usuario usuario)
        {
            comando.Parameters.AddWithValue(
                "@Email",
                usuario.Email ?? string.Empty
            );
            
             comando.Parameters.AddWithValue(
                "@NombreUsuario",
                ValorODbNull(usuario.NombreUsuario) 
              );   

            comando.Parameters.AddWithValue(
                "@Password",
                usuario.Password ?? string.Empty
            );

            comando.Parameters.AddWithValue(
                "@Roles",
                (int)usuario.Roles
            );

            comando.Parameters.AddWithValue(
                "@Estado",
                usuario.Estado
            );

            comando.Parameters.AddWithValue(
                "@Avatar",
                ValorODbNull(usuario.Avatar)
            );
        }

        private static Usuario Mapear(MySqlDataReader reader)
        {
            return new Usuario
            {
                IdUsuario = reader.GetInt32("IdUsuario"),
                Email = reader.GetString("Email"),
                NombreUsuario = reader.IsDBNull(reader.GetOrdinal("NombreUsuario"))? null
                    : reader.GetString("NombreUsuario"),
                Password = reader.GetString("Password"),
                Roles = (Roles)reader.GetInt32("Roles"),
                Estado = reader.GetBoolean("Estado"),
                Avatar = reader.IsDBNull(
                    reader.GetOrdinal("Avatar"))
                    ? null
                    : reader.GetString("Avatar")
            };
        }
    }
}
