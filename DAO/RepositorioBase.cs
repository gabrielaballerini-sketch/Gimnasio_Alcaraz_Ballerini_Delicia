using MySql.Data.MySqlClient;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
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
        protected static string? LeerStringNull(MySqlDataReader r, string col)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? null : r.GetString(i);
        }

        protected static bool LeerBool(MySqlDataReader r, string col, bool valorPorDefecto = true)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? valorPorDefecto : r.GetBoolean(i);
        }
    }
}