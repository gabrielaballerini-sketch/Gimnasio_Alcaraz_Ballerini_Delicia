using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using MySql.Data.MySqlClient;

public class RepositorioActividad : RepositorioBase, IRepositorio<Actividad>
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
                (Nombre, Estado)
            VALUES
                (@Nombre, @Estado);

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
            SET Nombre = @Nombre
            WHERE IdActividad = @IdActividad;";

        using var comand = new MySqlCommand(sql, conexion);

        comand.Parameters.AddWithValue(
            "@Nombre",
            actividad.Nombre ?? string.Empty
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
                Estado
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
        if (pagina < 1)
            pagina = 1;

        if (tamPagina < 1)
            tamPagina = 10;

        IList<Actividad> lista =
            new List<Actividad>();

        using var conexion = CrearConexion();

        var sql = @"
            SELECT
                IdActividad,
                Nombre,
                Estado
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
        if (pagina < 1)
            pagina = 1;

        if (tamPagina < 1)
            tamPagina = 10;

        IList<Actividad> lista =
            new List<Actividad>();

        using var conexion = CrearConexion();

        var sql = @"
            SELECT
                IdActividad,
                Nombre,
                Estado
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
                r.GetBoolean("Estado")
        };
    }
}