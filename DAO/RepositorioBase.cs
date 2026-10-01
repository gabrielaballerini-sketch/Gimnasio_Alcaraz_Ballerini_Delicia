using MySql.Data.MySqlClient;

public abstract class RepositorioBase
{
    private readonly string _connectionString;

    protected RepositorioBase(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'DefaultConnection' en appsettings.json");
    }

    protected MySqlConnection CrearConexion() => new MySqlConnection(_connectionString);

    // Convierte null en DBNull para poder usarlo como parámetro
    protected static object ValorODbNull(object? valor) => valor ?? DBNull.Value;

    // Lectura de columnas nullable
    protected static int? LeerIntNull(MySqlDataReader r, string col)
    {
        int i = r.GetOrdinal(col);
        return r.IsDBNull(i) ? null : r.GetInt32(i);
    }
}