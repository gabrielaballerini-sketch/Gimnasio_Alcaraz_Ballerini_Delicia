using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using MySql.Data.MySqlClient;

public class RepositorioProfesor : RepositorioBase, IRepositorioProfesor
{
    public RepositorioProfesor(IConfiguration configuration)
        : base(configuration)
    {
    }

    public int Alta(Profesor profesor)
    {
        int res = -1;

        using var conexion = CrearConexion();

        var sql = @"
            INSERT INTO profesor
                (Nombre, Apellido, Dni, Telefono, Domicilio, Estado)
            VALUES
                (@Nombre, @Apellido, @Dni, @Telefono, @Domicilio, @Estado);

            SELECT LAST_INSERT_ID();";

        using var comand = new MySqlCommand(sql, conexion);

        CargarParametros(comand, profesor);

        conexion.Open();

        res = Convert.ToInt32(comand.ExecuteScalar());

        profesor.IdProfesor = res;

        return res;
    }

    public int Modificacion(Profesor profesor)
    {
        int res = -1;

        using var conexion = CrearConexion();

        var sql = @"
            UPDATE profesor
            SET Nombre = @Nombre,
                Apellido = @Apellido,
                Dni = @Dni,
                Telefono = @Telefono,
                Domicilio = @Domicilio
            WHERE IdProfesor = @IdProfesor;";

        using var comand = new MySqlCommand(sql, conexion);

        CargarParametros(comand, profesor);

        comand.Parameters.AddWithValue("@IdProfesor", profesor.IdProfesor);

        conexion.Open();

        res = comand.ExecuteNonQuery();

        return res;
    }

    // Baja lógica
    public int Baja(int id)
    {
        int res = -1;

        using var conexion = CrearConexion();

        var sql = @"
            UPDATE profesor
            SET Estado = 0
            WHERE IdProfesor = @id;";

        using var comand = new MySqlCommand(sql, conexion);

        comand.Parameters.AddWithValue("@id", id);

        conexion.Open();

        res = comand.ExecuteNonQuery();

        return res;
    }

    public int Reactivar(int id)
    {
        int filas = 0;

        using var conexion = CrearConexion();

        var sql = @"
            UPDATE profesor
            SET Estado = 1
            WHERE IdProfesor = @id
              AND Estado = 0;";

        using var comand = new MySqlCommand(sql, conexion);

        comand.Parameters.AddWithValue("@id", id);

        conexion.Open();

        filas = comand.ExecuteNonQuery();

        return filas;
    }

    public Profesor? ObtenerPorId(int id)
    {
        Profesor? profesor = null;

        using var conexion = CrearConexion();

        var sql = @"
            SELECT IdProfesor,
                   Nombre,
                   Apellido,
                   Dni,
                   Telefono,
                   Domicilio,
                   Estado
            FROM profesor
            WHERE IdProfesor = @IdProfesor;";

        using var comand = new MySqlCommand(sql, conexion);

        comand.Parameters.AddWithValue("@IdProfesor", id);

        conexion.Open();

        using var read = comand.ExecuteReader();

        profesor = read.Read()
            ? Mapear(read)
            : null;

        return profesor;
    }

    public Profesor? ObtenerPorDni(string dni)
    {
        Profesor? profesor = null;

        using var conexion = CrearConexion();

        var sql = @"
            SELECT IdProfesor,
                   Nombre,
                   Apellido,
                   Dni,
                   Telefono,
                   Domicilio,
                   Estado
            FROM profesor
            WHERE Dni = @Dni;";

        using var comand = new MySqlCommand(sql, conexion);

        comand.Parameters.AddWithValue("@Dni", dni);

        conexion.Open();

        using var read = comand.ExecuteReader();

        profesor = read.Read()
            ? Mapear(read)
            : null;

        return profesor;
    }

    public bool ExisteDni(string dni, int? excluirId = null)
    {
        bool existe = false;

        using var conexion = CrearConexion();

        var sql = @"
            SELECT COUNT(*)
            FROM profesor
            WHERE Dni = @Dni
              AND (@ExcluirId IS NULL OR IdProfesor <> @ExcluirId);";

        using var comand = new MySqlCommand(sql, conexion);

        comand.Parameters.AddWithValue("@Dni", dni);
        comand.Parameters.AddWithValue(
            "@ExcluirId",
            ValorODbNull(excluirId)
        );

        conexion.Open();

        existe = Convert.ToInt32(comand.ExecuteScalar()) > 0;

        return existe;
    }

    public IList<Profesor> ObtenerActivos(
        int pagina = 1,
        int tamPagina = 10)
    {
        if (pagina < 1)
            pagina = 1;

        if (tamPagina < 1)
            tamPagina = 10;

        IList<Profesor> lista = new List<Profesor>();

        using var conexion = CrearConexion();

        var sql = @"
            SELECT IdProfesor,
                   Nombre,
                   Apellido,
                   Dni,
                   Telefono,
                   Domicilio,
                   Estado
            FROM profesor
            WHERE Estado = 1
            ORDER BY Apellido, Nombre, IdProfesor
            LIMIT @tamPagina
            OFFSET @desplazamiento;";

        using var comand = new MySqlCommand(sql, conexion);

        comand.Parameters.AddWithValue("@tamPagina", tamPagina);
        comand.Parameters.AddWithValue(
            "@desplazamiento",
            (pagina - 1) * tamPagina
        );

        conexion.Open();

        using var read = comand.ExecuteReader();

        while (read.Read())
        {
            lista.Add(Mapear(read));
        }

        return lista;
    }

    public IList<Profesor> ObtenerInactivos(
        int pagina = 1,
        int tamPagina = 10)
    {
        if (pagina < 1)
            pagina = 1;

        if (tamPagina < 1)
            tamPagina = 10;

        IList<Profesor> lista = new List<Profesor>();

        using var conexion = CrearConexion();

        var sql = @"
            SELECT IdProfesor,
                   Nombre,
                   Apellido,
                   Dni,
                   Telefono,
                   Domicilio,
                   Estado
            FROM profesor
            WHERE Estado = 0
            ORDER BY Apellido, Nombre, IdProfesor
            LIMIT @tamPagina
            OFFSET @desplazamiento;";

        using var comand = new MySqlCommand(sql, conexion);

        comand.Parameters.AddWithValue("@tamPagina", tamPagina);
        comand.Parameters.AddWithValue(
            "@desplazamiento",
            (pagina - 1) * tamPagina
        );

        conexion.Open();

        using var read = comand.ExecuteReader();

        while (read.Read())
        {
            lista.Add(Mapear(read));
        }

        return lista;
    }

    public int ObtenerCantidad(bool? soloActivos = true)
    {
        int cantidad = 0;

        using var conexion = CrearConexion();

        var sql = @"
            SELECT COUNT(*)
            FROM profesor
            WHERE (@Estado IS NULL OR Estado = @Estado);";

        using var comand = new MySqlCommand(sql, conexion);

        comand.Parameters.AddWithValue(
            "@Estado",
            ValorODbNull(soloActivos)
        );

        conexion.Open();

        cantidad = Convert.ToInt32(comand.ExecuteScalar());

        return cantidad;
    }

    private static void CargarParametros(
        MySqlCommand cmd,
        Profesor profesor)
    {
        cmd.Parameters.AddWithValue(
            "@Nombre",
            profesor.Nombre ?? string.Empty
        );

        cmd.Parameters.AddWithValue(
            "@Apellido",
            profesor.Apellido ?? string.Empty
        );

        cmd.Parameters.AddWithValue(
            "@Dni",
            profesor.Dni ?? string.Empty
        );

        cmd.Parameters.AddWithValue(
            "@Telefono",
            profesor.Telefono ?? string.Empty
        );

        cmd.Parameters.AddWithValue(
            "@Domicilio",
            profesor.Domicilio ?? string.Empty
        );

        cmd.Parameters.AddWithValue(
            "@Estado",
            profesor.Estado
        );
    }

    private static Profesor Mapear(MySqlDataReader r) => new Profesor
    {
        IdProfesor = r.GetInt32("IdProfesor"),
        Nombre = r.GetString("Nombre"),
        Apellido = r.GetString("Apellido"),
        Dni = r.GetString("Dni"),
        Telefono = r.GetString("Telefono"),
        Domicilio = r.GetString("Domicilio"),
        Estado = r.GetBoolean("Estado")
    };
}