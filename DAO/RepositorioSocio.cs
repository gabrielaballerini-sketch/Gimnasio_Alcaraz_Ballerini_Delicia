using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using MySql.Data.MySqlClient;

public class RepositorioSocio : RepositorioBase
{
    public RepositorioSocio(IConfiguration configuration) : base(configuration) { }

    private const string Columnas = "id_socio, nombre, apellido, dni, telefono, domicilio, avatar, estado";

    public int Alta(Socio socio)
    {   int res =-1;
        using var conexion = CrearConexion();
     
        var sql = @"INSERT INTO socio (Nombre, Apellido, Dni, Telefono, Domicilio, Avatar, Estado)
                    VALUES (@Nombre, @Apellido, @Dni, @Telefono, @Domicilio, @Avatar, @Estado);
                    SELECT LAST_INSERT_ID();";
        using var comand = new MySqlCommand(sql, conexion);
        
        CargarParametros(comand, socio);
        conexion.Open();
        res= Convert.ToInt32(comand.ExecuteScalar());
        socio.IdSocio=res;
       
        
        return res;
    }

    public int Modificar(Socio socio)
    {
        int res= -1;
        using var conexion = CrearConexion();
    
        var sql = @"UPDATE socio SET Nombre=@Nombre, Apellido=@Apellido, Dni=@Dni, Telefono=@Telefono,
                           Domicilio=@Domicilio, Avatar=@Avatar
                    WHERE IdSocio=@IdSocio";
        using var comand = new MySqlCommand(sql, conexion);
        
        CargarParametros(comand, socio);
        conexion.Open();

        comand.Parameters.AddWithValue("@IdSocio", socio.IdSocio);
        res=comand.ExecuteNonQuery();
      
        return res;
    }

    // Baja lógica
    public int Baja(int id)
    {   int res=-1;
        using var conexion = CrearConexion();
        conexion.Open();

        string sql=@"UPDATE socio SET Estado=0 WHERE IdSocio=@id";


        using var comand = new MySqlCommand(sql, conexion);
        comand.Parameters.AddWithValue("@id", id);
        res=comand.ExecuteNonQuery();
      
        return res;
    }

    public Socio? ObtenerPorId(int IdSocio)
    {   
        Socio? socio = null;
        using var conexion = CrearConexion();
      
        string sql=@"SELECT IdSocio, Nombre, Apellido, Dni, Telefono, Domicilio, Avatar, Estado FROM socio WHERE IdSocio=@IdSocio";

        using var comand = new MySqlCommand(sql, conexion);
      
        comand.Parameters.AddWithValue("@IdSocio", IdSocio);
        conexion.Open();
        using var read = comand.ExecuteReader();
        
        socio=read.Read()? Mapear(read) : null;
      


        return socio;
    }

    public Socio? ObtenerPorDni(string dni)
    {
        Socio? socio = null;
        using var conexion = CrearConexion();
      
        string sql=@"SELECT IdSocio, Nombre, Apellido, Dni, Telefono, Domicilio, Avatar, Estado FROM socio WHERE Dni=@dni";
        using var comand = new MySqlCommand(sql, conexion);
        comand.Parameters.AddWithValue("@dni", dni);
        conexion.Open();
        using var read = comand.ExecuteReader();
       
        socio=read.Read()? Mapear(read) : null;
        
        return socio;
    }

    public bool ExisteDni(string dni, int? excluirId = null)
    {  bool res=false;
        using var conexion = CrearConexion();
      
        string sql=@"SELECT COUNT(*) FROM socio WHERE Dni=@Dni AND (@excluir IS NULL OR IdSocio<>@excluir)";
        using var comand = new MySqlCommand(sql, conexion);

        comand.Parameters.AddWithValue("@Dni", dni);
        
        comand.Parameters.AddWithValue("@excluir", ValorODbNull(excluirId));
        conexion.Open();
        res=Convert.ToInt32(comand.ExecuteScalar()) > 0;
        return res;
    }

    // Listado de socios, filtrando opcionalmente por estado (null = todos)
    public List<Socio> ObtenerTodos(bool? estado = null)
    {
        var lista = new List<Socio>();
        using var conexion = CrearConexion();
        
        string sql=@"SELECT IdSocio, Nombre, Apellido, Dni, Telefono, Domicilio, Avatar, Estado FROM socio WHERE (@Estado IS NULL OR Estado=@Estado) ORDER BY Apellido, Nombre";

        using var comand = new MySqlCommand(sql, conexion);
        comand.Parameters.AddWithValue("@estado", ValorODbNull(estado));
        conexion.Open();
        using var read = comand.ExecuteReader();
        while (read.Read()) lista.Add(Mapear(read));
        
        return lista;
    }

    private static void CargarParametros(MySqlCommand cmd, Socio s)
    {
        cmd.Parameters.AddWithValue("@nombre", s.Nombre ?? string.Empty);
        cmd.Parameters.AddWithValue("@apellido", s.Apellido ?? string.Empty);
        cmd.Parameters.AddWithValue("@dni", s.Dni ?? string.Empty);
        cmd.Parameters.AddWithValue("@telefono", s.Telefono ?? string.Empty);
        cmd.Parameters.AddWithValue("@domicilio", s.Domicilio ?? string.Empty);
        cmd.Parameters.AddWithValue("@avatar", s.Avatar ?? string.Empty);
        cmd.Parameters.AddWithValue("@estado", s.Estado);
    }

    private static Socio Mapear(MySqlDataReader r) => new Socio
    {
        IdSocio = r.GetInt32("id_socio"),
        Nombre = r.GetString("nombre"),
        Apellido = r.GetString("apellido"),
        Dni = r.GetString("dni"),
        Telefono = r.GetString("telefono"),
        Domicilio = r.GetString("domicilio"),
        Avatar = r.GetString("avatar"),
        Estado = r.GetBoolean("estado")
    };
}