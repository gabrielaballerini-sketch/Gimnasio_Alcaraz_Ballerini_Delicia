using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using MySql.Data.MySqlClient;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.DAO
{
    public class RepositorioActividadArchivo : RepositorioBase, IRepositorioActividadArchivo
    {
        public RepositorioActividadArchivo(IConfiguration configuration)
            : base(configuration)
        {
        }

        public int Alta(ActividadArchivo archivo)
        {
            using var conexion = CrearConexion();

            var sql = @"
            INSERT INTO actividad_archivo
                (IdActividad, Ruta)
            VALUES
                (@IdActividad, @Ruta);

            SELECT LAST_INSERT_ID();";

            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue("@IdActividad", archivo.IdActividad);
            comand.Parameters.AddWithValue("@Ruta", archivo.Ruta);

            conexion.Open();

            var id = Convert.ToInt32(comand.ExecuteScalar());
            archivo.IdArchivo = id;

            return id;
        }


        public int Eliminar(int idArchivo)
        {
            using var conexion = CrearConexion();

            var sql = @"
            DELETE FROM actividad_archivo
            WHERE IdArchivo = @IdArchivo;";

            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue("@IdArchivo", idArchivo);

            conexion.Open();

            return comand.ExecuteNonQuery();
        }


        public ActividadArchivo? ObtenerPorId(int idArchivo)
        {
            using var conexion = CrearConexion();

            var sql = @"
            SELECT IdArchivo, IdActividad, Ruta
            FROM actividad_archivo
            WHERE IdArchivo = @IdArchivo;";

            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue("@IdArchivo", idArchivo);

            conexion.Open();

            using var read = comand.ExecuteReader();

            return read.Read() ? Mapear(read) : null;
        }


        public IList<ActividadArchivo> ObtenerPorActividad(int idActividad)
        {
            IList<ActividadArchivo> lista = new List<ActividadArchivo>();

            using var conexion = CrearConexion();

            var sql = @"
            SELECT IdArchivo, IdActividad, Ruta
            FROM actividad_archivo
            WHERE IdActividad = @IdActividad
            ORDER BY IdArchivo;";

            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue("@IdActividad", idActividad);

            conexion.Open();

            using var read = comand.ExecuteReader();

            while (read.Read())
            {
                lista.Add(Mapear(read));
            }

            return lista;
        }


        public int Contar(int idActividad)
        {
            using var conexion = CrearConexion();

            var sql = @"
            SELECT COUNT(*)
            FROM actividad_archivo
            WHERE IdActividad = @IdActividad;";

            using var comand = new MySqlCommand(sql, conexion);

            comand.Parameters.AddWithValue("@IdActividad", idActividad);

            conexion.Open();

            return Convert.ToInt32(comand.ExecuteScalar());
        }


        private static ActividadArchivo Mapear(MySqlDataReader r)
        {
            return new ActividadArchivo
            {
                IdArchivo = r.GetInt32("IdArchivo"),
                IdActividad = r.GetInt32("IdActividad"),
                Ruta = r.GetString("Ruta")
            };
        }
    }
}
