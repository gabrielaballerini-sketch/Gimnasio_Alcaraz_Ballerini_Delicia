using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using MySql.Data.MySqlClient;

public class RepositorioUsuario : RepositorioBase, IRepositorioUsuario
{
    public RepositorioUsuario(IConfiguration configuration)
        : base(configuration)
    {
    }

    public int Alta(Usuario usuario)
    {
        int res = -1;

        using var conexion = CrearConexion();

        var sql = @"
            INSERT INTO usuario
                (Email, Password, Roles,Estado)
            VALUES
                (@Email, @Password, @Roles,@Estado);

            SELECT LAST_INSERT_ID();";

        using var comand = new MySqlCommand(sql, conexion);

        CargarParametros(comand, usuario);

        conexion.Open();

        res = Convert.ToInt32(comand.ExecuteScalar());

        usuario.IdUsuario = res;

        return res;
    }

    public int Modificacion(Usuario usuario)
    {
        int res = -1;

        using var conexion = CrearConexion();

        var sql = @"
            UPDATE usuario
            SET Email = @Email,
                Password = @Password,
                Roles = @Roles,
                Estado=@Estado
            WHERE IdUsuario = @IdUsuario;";

        using var comand = new MySqlCommand(sql, conexion);

        CargarParametros(comand, usuario);

        comand.Parameters.AddWithValue(
            "@IdUsuario",
            usuario.IdUsuario
        );

        conexion.Open();

        res = comand.ExecuteNonQuery();

        return res;
    }

    public int Baja(int id)
    {
        int res = -1;

        using var conexion = CrearConexion();

        var sql = @"
            UPDATE usuario
            SET Estado = 0
            WHERE IdUsuario = @id;";

        using var comand = new MySqlCommand(sql, conexion);

        comand.Parameters.AddWithValue("@id", id);

        conexion.Open();

        res = comand.ExecuteNonQuery();

        return res;
    }

    public int Reactivar(int id)
    {
        int res = 0;

        using var conexion = CrearConexion();

        var sql = @"
            UPDATE usuario
            SET Estado = 1
            WHERE IdUsuario = @id
              AND Estado = 0;";

        using var comand = new MySqlCommand(sql, conexion);

        comand.Parameters.AddWithValue("@id", id);

        conexion.Open();

        res = comand.ExecuteNonQuery();

        return res;
    }

    public Usuario? ObtenerPorId(int id)
    {
        Usuario? usuario = null;

        using var conexion = CrearConexion();

        var sql = @"
            SELECT IdUsuario,
                   Email,
                   Password,
                   Roles,
                   Estado
            FROM usuario
            WHERE IdUsuario = @IdUsuario;";

        using var comand = new MySqlCommand(sql, conexion);

        comand.Parameters.AddWithValue(
            "@IdUsuario",
            id
        );

        conexion.Open();

        using var read = comand.ExecuteReader();

        usuario = read.Read()
            ? Mapear(read)
            : null;

        return usuario;
    }

    public Usuario? ObtenerPorEmail(string email)
    {
        Usuario? usuario = null;

        using var conexion = CrearConexion();

        var sql = @"
            SELECT IdUsuario,
                   Email,
                   Password,
                   Roles,
                   Estado
            FROM usuario
            WHERE Email = @Email;";

        using var comand = new MySqlCommand(sql, conexion);

        comand.Parameters.AddWithValue(
            "@Email",
            email
        );

        conexion.Open();

        using var read = comand.ExecuteReader();

        usuario = read.Read()
            ? Mapear(read)
            : null;

        return usuario;
    }

    public bool ExisteEmail(
        string email,
        int? excluirId = null)
    {
        bool existe = false;

        using var conexion = CrearConexion();

        var sql = @"
            SELECT COUNT(*)
            FROM usuario
            WHERE Email = @Email
              AND (@ExcluirId IS NULL
                   OR IdUsuario <> @ExcluirId);";

        using var comand = new MySqlCommand(sql, conexion);

        comand.Parameters.AddWithValue(
            "@Email",
            email
        );

        comand.Parameters.AddWithValue(
            "@ExcluirId",
            ValorODbNull(excluirId)
        );

        conexion.Open();

        existe =
            Convert.ToInt32(
                comand.ExecuteScalar()
            ) > 0;

        return existe;
    }

    public IList<Usuario> ObtenerActivos(
        int pagina = 1,
        int tamPagina = 10)
    {
        if (pagina < 1)
            pagina = 1;

        if (tamPagina < 1)
            tamPagina = 10;

        IList<Usuario> lista =
            new List<Usuario>();

        using var conexion = CrearConexion();

        var sql = @"
            SELECT IdUsuario,
                   Email,
                   Password,
                   Roles,
                   Estado
            FROM usuario
            WHERE Estado = 1
            ORDER BY Email, IdUsuario
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
            lista.Add(Mapear(read));
        }

        return lista;
    }

    public IList<Usuario> ObtenerInactivos(
        int pagina = 1,
        int tamPagina = 10)
    {
        if (pagina < 1)
            pagina = 1;

        if (tamPagina < 1)
            tamPagina = 10;

        IList<Usuario> lista =
            new List<Usuario>();

        using var conexion = CrearConexion();

        var sql = @"
            SELECT IdUsuario,
                   Email,
                   Password,
                   Roles,
                   Estado
            FROM usuario
            WHERE Estado = 0
            ORDER BY Email, IdUsuario
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
            lista.Add(Mapear(read));
        }

        return lista;
    }

    public int ObtenerCantidad(
        bool? soloActivos = true)
    {
        int cantidad = 0;

        using var conexion = CrearConexion();

        var sql = @"
            SELECT COUNT(*)
            FROM usuario
            WHERE (@Estado IS NULL
                   OR Estado = @Estado);";

        using var comand =
            new MySqlCommand(sql, conexion);

        comand.Parameters.AddWithValue(
            "@Estado",
            ValorODbNull(soloActivos)
        );

        conexion.Open();

        cantidad =
            Convert.ToInt32(
                comand.ExecuteScalar()
            );

        return cantidad;
    }

    private static void CargarParametros(
        MySqlCommand cmd,
        Usuario usuario)
    {
        cmd.Parameters.AddWithValue(
            "@Email",
            usuario.Email ?? string.Empty
        );

        cmd.Parameters.AddWithValue(
            "@Password",
            usuario.Password ?? string.Empty
        );

        cmd.Parameters.AddWithValue(
            "@Roles",
            (int)usuario.Roles
        );
         cmd.Parameters.AddWithValue(
         "@Estado",
          usuario.Estado
         );


    }

    private static Usuario Mapear(
        MySqlDataReader r)
    {
        return new Usuario
        {
            IdUsuario =
                r.GetInt32("IdUsuario"),

            Email =
                r.GetString("Email"),

            Password =
                r.GetString("Password"),

            Roles =
                (Roles)r.GetInt32("Roles"),

            Estado=r.GetBoolean("Estado")


        };
    }
}