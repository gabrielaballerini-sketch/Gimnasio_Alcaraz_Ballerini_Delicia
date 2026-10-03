using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using MySql.Data.MySqlClient;

public class RepositorioPlan : RepositorioBase, IRepositorio<Plan>
{
    public RepositorioPlan(IConfiguration configuration)
        : base(configuration)
    {
    }

    public int Alta(Plan plan)
    {
        using var conexion = CrearConexion();

        var sql = @"
            INSERT INTO plan
                (Nombre, UtilizacionesMensuales, Precio, Estado)
            VALUES
                (@Nombre, @UtilizacionesMensuales, @Precio, @Estado);

            SELECT LAST_INSERT_ID();";

        using var comand = new MySqlCommand(sql, conexion);

        CargarParametros(comand, plan);

        conexion.Open();

        var id = Convert.ToInt32(comand.ExecuteScalar());

        plan.IdPlan = id;

        return id;
    }

    public int Modificacion(Plan plan)
    {
        using var conexion = CrearConexion();

        var sql = @"
            UPDATE plan
            SET Nombre = @Nombre,
                UtilizacionesMensuales = @UtilizacionesMensuales,
                Precio = @Precio
            WHERE IdPlan = @IdPlan;";

        using var comand = new MySqlCommand(sql, conexion);

        CargarParametros(comand, plan);

        comand.Parameters.AddWithValue(
            "@IdPlan",
            plan.IdPlan
        );

        conexion.Open();

        return comand.ExecuteNonQuery();
    }

    public int Baja(int id)
    {
        using var conexion = CrearConexion();

        var sql = @"
            UPDATE plan
            SET Estado = 0
            WHERE IdPlan = @IdPlan;";

        using var comand = new MySqlCommand(sql, conexion);

        comand.Parameters.AddWithValue("@IdPlan", id);

        conexion.Open();

        return comand.ExecuteNonQuery();
    }

    public int Reactivar(int id)
    {
        using var conexion = CrearConexion();

        var sql = @"
            UPDATE plan
            SET Estado = 1
            WHERE IdPlan = @IdPlan
              AND Estado = 0;";

        using var comand = new MySqlCommand(sql, conexion);

        comand.Parameters.AddWithValue("@IdPlan", id);

        conexion.Open();

        return comand.ExecuteNonQuery();
    }

    public Plan? ObtenerPorId(int id)
    {
        using var conexion = CrearConexion();

        var sql = @"
            SELECT IdPlan,
                   Nombre,
                   UtilizacionesMensuales,
                   Precio,
                   Estado
            FROM plan
            WHERE IdPlan = @IdPlan;";

        using var comand = new MySqlCommand(sql, conexion);

        comand.Parameters.AddWithValue("@IdPlan", id);

        conexion.Open();

        using var read = comand.ExecuteReader();

        return read.Read()
            ? Mapear(read)
            : null;
    }

    public IList<Plan> ObtenerActivos(
        int pagina = 1,
        int tamPagina = 10)
    {
        if (pagina < 1)
            pagina = 1;

        if (tamPagina < 1)
            tamPagina = 10;

        IList<Plan> lista = new List<Plan>();

        using var conexion = CrearConexion();

        var sql = @"
            SELECT IdPlan,
                   Nombre,
                   UtilizacionesMensuales,
                   Precio,
                   Estado
            FROM plan
            WHERE Estado = 1
            ORDER BY Nombre, IdPlan
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

        using var read = comand.ExecuteReader();

        while (read.Read())
        {
            lista.Add(Mapear(read));
        }

        return lista;
    }

    public IList<Plan> ObtenerInactivos(
        int pagina = 1,
        int tamPagina = 10)
    {
        if (pagina < 1)
            pagina = 1;

        if (tamPagina < 1)
            tamPagina = 10;

        IList<Plan> lista = new List<Plan>();

        using var conexion = CrearConexion();

        var sql = @"
            SELECT IdPlan,
                   Nombre,
                   UtilizacionesMensuales,
                   Precio,
                   Estado
            FROM plan
            WHERE Estado = 0
            ORDER BY Nombre, IdPlan
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

        using var read = comand.ExecuteReader();

        while (read.Read())
        {
            lista.Add(Mapear(read));
        }

        return lista;
    }

    public int ObtenerCantidad(bool? soloActivos = true)
    {
        using var conexion = CrearConexion();

        var sql = @"
            SELECT COUNT(*)
            FROM plan
            WHERE (@Estado IS NULL OR Estado = @Estado);";

        using var comand = new MySqlCommand(sql, conexion);

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
        Plan plan)
    {
        cmd.Parameters.AddWithValue(
            "@Nombre",
            plan.Nombre ?? string.Empty
        );

        cmd.Parameters.AddWithValue(
            "@UtilizacionesMensuales",
            ValorODbNull(plan.UtilizacionesMensuales)
        );

        cmd.Parameters.AddWithValue(
            "@Precio",
            plan.Precio
        );

        cmd.Parameters.AddWithValue(
            "@Estado",
            plan.Estado
        );
    }

    private static Plan Mapear(MySqlDataReader r)
    {
        return new Plan
        {
            IdPlan = r.GetInt32("IdPlan"),

            Nombre = r.GetString("Nombre"),

            UtilizacionesMensuales =
                r.IsDBNull(
                    r.GetOrdinal("UtilizacionesMensuales")
                )
                    ? null
                    : r.GetInt32("UtilizacionesMensuales"),

            Precio = r.GetDecimal("Precio"),

            Estado = r.GetBoolean("Estado")
        };
    }
}