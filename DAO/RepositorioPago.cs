using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using MySql.Data.MySqlClient;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
    public class RepositorioPago : RepositorioBase, IRepositorioPago
    {
        public RepositorioPago(IConfiguration configuration)
            : base(configuration)
        {
        }

        public int Alta(Pago pago)
        {
            using var conexion = CrearConexion();

            const string sql = @"
                INSERT INTO pago(IdMembresia, Fecha, Importe,MedioPago, Estado, IdUsuarioCreador,
                     IdUsuarioAnulador)
                VALUES(@IdMembresia, @Fecha, @Importe,@MedioPago, 1, @IdUsuarioCreador, NULL);

                SELECT LAST_INSERT_ID();";

            using var comando = new MySqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("@IdMembresia", pago.IdMembresia);
            comando.Parameters.AddWithValue("@Fecha", pago.Fecha);
            comando.Parameters.AddWithValue("@Importe", pago.Importe);
            comando.Parameters.AddWithValue("@MedioPago", (int)pago.MedioPago);
            comando.Parameters.AddWithValue("@IdUsuarioCreador", pago.IdUsuarioCreador);

            conexion.Open();

            pago.IdPago = Convert.ToInt32(comando.ExecuteScalar());
            pago.Estado = true;

            return pago.IdPago;
        }

      
public int RegistrarPagoYHabilitarMembresia(
    Pago pago,
    DateTime fechaInicio,
    DateTime fechaFin)
{
    using var conexion = CrearConexion();
    conexion.Open();

    using var transaccion = conexion.BeginTransaction();

    try
    {
        const string sqlBloqueo = @" SELECT Estado, Pagada, FechaInicio, FechaFin
            FROM membresia
            WHERE IdMembresia = @IdMembresia
            FOR UPDATE;";

        bool estado;
        bool pagada;
        DateTime inicioActual;
        DateTime finActual;

        using (var comando = new MySqlCommand(
            sqlBloqueo, conexion, transaccion))
        {
            comando.Parameters.AddWithValue( "@IdMembresia", pago.IdMembresia);

            using var reader = comando.ExecuteReader();

            if (!reader.Read())
                throw new InvalidOperationException(
                    "No se encontró la membresía.");

            estado = reader.GetBoolean("Estado");
            pagada = reader.GetBoolean("Pagada");
            inicioActual = reader.GetDateTime("FechaInicio");
            finActual = reader.GetDateTime("FechaFin");
        }

        if (!estado)
            throw new InvalidOperationException( "No se puede cobrar una membresía dada de baja.");

        bool vigente =
            inicioActual.Date <= DateTime.Today && finActual.Date >= DateTime.Today;

        if (pagada && vigente)
            throw new InvalidOperationException(
                "La membresía ya está pagada y vigente.");

        const string sqlPago = @"
            INSERT INTO pago
            (
                IdMembresia,
                Fecha,
                Importe,
                MedioPago,
                Estado,
                IdUsuarioCreador,
                IdUsuarioAnulador
            )
            VALUES
            (
                @IdMembresia,
                @Fecha,
                @Importe,
                @MedioPago,
                1,
                @IdUsuarioCreador,
                NULL
            );

            SELECT LAST_INSERT_ID();";

        using (var comando = new MySqlCommand(
            sqlPago, conexion, transaccion))
        {
            comando.Parameters.AddWithValue( "@IdMembresia", pago.IdMembresia);
            comando.Parameters.AddWithValue("@Fecha", pago.Fecha);
            comando.Parameters.AddWithValue("@Importe", pago.Importe);
            comando.Parameters.AddWithValue( "@MedioPago", (int)pago.MedioPago);
            comando.Parameters.AddWithValue( "@IdUsuarioCreador", pago.IdUsuarioCreador);

            pago.IdPago = Convert.ToInt32(
                comando.ExecuteScalar());
        }

        const string sqlMembresia = @"  UPDATE membresia
            SET FechaInicio = @FechaInicio,
                FechaFin = @FechaFin,
                PrecioContratado = @Importe,
                Pagada = 1
            WHERE IdMembresia = @IdMembresia AND Estado = 1;";

        using (var comando = new MySqlCommand(
            sqlMembresia, conexion, transaccion))
        {
            comando.Parameters.AddWithValue(
                "@FechaInicio", fechaInicio.Date);
            comando.Parameters.AddWithValue(
                "@FechaFin", fechaFin.Date);

            // En una renovación, el precio contratado pasa a ser el del nuevo período.
            comando.Parameters.AddWithValue(
                "@Importe", pago.Importe);
            comando.Parameters.AddWithValue(
                "@IdMembresia", pago.IdMembresia);

            int filas = comando.ExecuteNonQuery();

            if (filas != 1)
                throw new InvalidOperationException( "No se pudo habilitar la membresía.");
        }

        transaccion.Commit();

        pago.Estado = true;

        return pago.IdPago;
    }
    catch
    {
        try
        {
            transaccion.Rollback();
        }
        catch
        {
            // Conservamos la excepción original.
        }

        throw;
    }
}


        public Pago? ObtenerPorId(int id)
        {
            using var conexion = CrearConexion();

            const string sql = @"
                SELECT
                    p.IdPago,
                    p.IdMembresia,
                    p.Fecha,
                    p.Importe,
                    p.MedioPago,
                    p.Estado,
                    p.IdUsuarioCreador,
                    p.IdUsuarioAnulador,
                    m.IdSocio,
                    s.Nombre AS SocioNombre,
                    s.Apellido AS SocioApellido
                FROM pago p
                INNER JOIN membresia m
                    ON m.IdMembresia = p.IdMembresia
                INNER JOIN socio s
                    ON s.IdSocio = m.IdSocio
                WHERE p.IdPago = @IdPago;";

            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@IdPago", id);

            conexion.Open();

            using var reader = comando.ExecuteReader();

            return reader.Read() ? Mapear(reader) : null;
        }

        public IList<Pago> ObtenerTodos(
            int pagina = 1,
            int tamPagina = 10)
        {
            pagina = Math.Max(1, pagina);
            tamPagina = Math.Max(1, tamPagina);

            var lista = new List<Pago>();

            using var conexion = CrearConexion();

            const string sql = @"
                SELECT
                    p.IdPago,
                    p.IdMembresia,
                    p.Fecha,
                    p.Importe,
                    p.MedioPago,
                    p.Estado,
                    p.IdUsuarioCreador,
                    p.IdUsuarioAnulador,
                    m.IdSocio,
                    s.Nombre AS SocioNombre,
                    s.Apellido AS SocioApellido
                FROM pago p
                INNER JOIN membresia m
                    ON m.IdMembresia = p.IdMembresia
                INNER JOIN socio s
                    ON s.IdSocio = m.IdSocio
                ORDER BY p.Fecha DESC, p.IdPago DESC
                LIMIT @TamPagina OFFSET @Desplazamiento;";

            using var comando = new MySqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("@TamPagina", tamPagina);
            comando.Parameters.AddWithValue(
                "@Desplazamiento",
                (pagina - 1) * tamPagina);

            conexion.Open();

            using var reader = comando.ExecuteReader();

            while (reader.Read())
                lista.Add(Mapear(reader));

            return lista;
        }

        public IList<Pago> ObtenerPorSocio(
            int idUsuario,
            int pagina = 1,
            int tamPagina = 10)
        {
            pagina = Math.Max(1, pagina);
            tamPagina = Math.Max(1, tamPagina);

            var lista = new List<Pago>();

            using var conexion = CrearConexion();

            const string sql = @"
                SELECT
                    p.IdPago,
                    p.IdMembresia,
                    p.Fecha,
                    p.Importe,
                    p.MedioPago,
                    p.Estado,
                    p.IdUsuarioCreador,
                    p.IdUsuarioAnulador,
                    m.IdSocio,
                    s.Nombre AS SocioNombre,
                    s.Apellido AS SocioApellido
                FROM pago p
                INNER JOIN membresia m
                    ON m.IdMembresia = p.IdMembresia
                INNER JOIN socio s
                    ON s.IdSocio = m.IdSocio
                WHERE s.IdUsuario = @IdUsuario
                ORDER BY p.Fecha DESC, p.IdPago DESC
                LIMIT @TamPagina OFFSET @Desplazamiento;";

            using var comando = new MySqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("@IdUsuario", idUsuario);
            comando.Parameters.AddWithValue("@TamPagina", tamPagina);
            comando.Parameters.AddWithValue(
                "@Desplazamiento",
                (pagina - 1) * tamPagina);

            conexion.Open();

            using var reader = comando.ExecuteReader();

            while (reader.Read())
                lista.Add(Mapear(reader));

            return lista;
        }

        public int ObtenerCantidad(bool? soloActivos = true)
        {
            using var conexion = CrearConexion();

            const string sql = @"
                SELECT COUNT(*)
                FROM pago
                WHERE (@Estado IS NULL OR Estado = @Estado);";

            using var comando = new MySqlCommand(sql, conexion);

            comando.Parameters.AddWithValue(
                "@Estado",
                soloActivos.HasValue
                    ? (object)(soloActivos.Value ? 1 : 0)
                    : DBNull.Value);

            conexion.Open();

            return Convert.ToInt32(comando.ExecuteScalar());
        }


        public int Anular(int idPago, int idUsuarioAnulador)
        {
            using var conexion = CrearConexion();

            const string sql = @"
                UPDATE pago
                SET Estado = 0,
                    IdUsuarioAnulador = @IdUsuarioAnulador
                WHERE IdPago = @IdPago
                  AND Estado = 1;";

            using var comando = new MySqlCommand(sql, conexion);

            comando.Parameters.AddWithValue(
                "@IdUsuarioAnulador", idUsuarioAnulador);
            comando.Parameters.AddWithValue("@IdPago", idPago);

            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        private static Pago Mapear(MySqlDataReader reader)
        {
            return new Pago
            {
                IdPago = reader.GetInt32("IdPago"),
                IdMembresia = reader.GetInt32("IdMembresia"),
                Fecha = reader.GetDateTime("Fecha"),
                Importe = reader.GetDecimal("Importe"),
                MedioPago = (MedioPago)reader.GetInt32("MedioPago"),
                Estado = reader.GetBoolean("Estado"),
                IdUsuarioCreador = reader.GetInt32("IdUsuarioCreador"),
                IdUsuarioAnulador = reader.IsDBNull(
                    reader.GetOrdinal("IdUsuarioAnulador"))
                        ? null
                        : reader.GetInt32("IdUsuarioAnulador"),

                Membresia = new Membresia
                {
                    IdMembresia = reader.GetInt32("IdMembresia"),
                    IdSocio = reader.GetInt32("IdSocio"),
                    Socio = new Socio
                    {
                        IdSocio = reader.GetInt32("IdSocio"),
                        Nombre = reader.GetString("SocioNombre"),
                        Apellido = reader.GetString("SocioApellido")
                    }
                }
            };
        

        }

    }
}