using MySqlConnector;

namespace GestionClientesSemana3;

//cuatro procedimientos del CRUD con consultas preparadas.
public class ClienteRepositorio
{
    private readonly string _cadenaConexion;

    public ClienteRepositorio(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    //consultar todos los clientes.
    public async Task<List<Cliente>> ListarAsync()
    {
        var clientes = new List<Cliente>();
        await using var conexion = new MySqlConnection(_cadenaConexion);
        await conexion.OpenAsync();

        const string sql = """
            SELECT id, nombres, apellidos, identificacion, correo, telefono,
                   direccion, ciudad, provincia, fecha_nacimiento, estado
            FROM clientes ORDER BY id DESC;
            """;
        await using var comando = new MySqlCommand(sql, conexion);
        await using var lector = await comando.ExecuteReaderAsync();
        while (await lector.ReadAsync())
        {
            clientes.Add(LeerCliente(lector));
        }
        return clientes;
    }

    //consultar un cliente por su ID para editarlo.
    public async Task<Cliente?> ObtenerAsync(int id)
    {
        await using var conexion = new MySqlConnection(_cadenaConexion);
        await conexion.OpenAsync();

        const string sql = """
            SELECT id, nombres, apellidos, identificacion, correo, telefono,
                   direccion, ciudad, provincia, fecha_nacimiento, estado
            FROM clientes WHERE id = @id;
            """;
        await using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@id", id);
        await using var lector = await comando.ExecuteReaderAsync();
        return await lector.ReadAsync() ? LeerCliente(lector) : null;
    }

    //registrar un nuevo cliente.
    public async Task<int> CrearAsync(Cliente cliente)
    {
        await using var conexion = new MySqlConnection(_cadenaConexion);
        await conexion.OpenAsync();

        const string sql = """
            INSERT INTO clientes
                (nombres, apellidos, identificacion, correo, telefono,
                 direccion, ciudad, provincia, fecha_nacimiento, estado)
            VALUES
                (@nombres, @apellidos, @identificacion, @correo, @telefono,
                 @direccion, @ciudad, @provincia, @fecha_nacimiento, @estado);
            """;
        await using var comando = new MySqlCommand(sql, conexion);
        AgregarParametros(comando, cliente);
        await comando.ExecuteNonQueryAsync();
        return (int)comando.LastInsertedId;
    }

    //modificar los datos de un cliente existente.
    public async Task<bool> ActualizarAsync(int id, Cliente cliente)
    {
        await using var conexion = new MySqlConnection(_cadenaConexion);
        await conexion.OpenAsync();

        const string sql = """
            UPDATE clientes SET
                nombres = @nombres, apellidos = @apellidos,
                identificacion = @identificacion, correo = @correo,
                telefono = @telefono, direccion = @direccion,
                ciudad = @ciudad, provincia = @provincia,
                fecha_nacimiento = @fecha_nacimiento, estado = @estado
            WHERE id = @id;
            """;
        await using var comando = new MySqlCommand(sql, conexion);
        AgregarParametros(comando, cliente);
        comando.Parameters.AddWithValue("@id", id);
        return await comando.ExecuteNonQueryAsync() > 0;
    }

    //eliminar un cliente por su ID.
    public async Task<bool> EliminarAsync(int id)
    {
        await using var conexion = new MySqlConnection(_cadenaConexion);
        await conexion.OpenAsync();

        await using var comando = new MySqlCommand(
            "DELETE FROM clientes WHERE id = @id;", conexion);
        comando.Parameters.AddWithValue("@id", id);
        return await comando.ExecuteNonQueryAsync() > 0;
    }

    private static void AgregarParametros(MySqlCommand comando, Cliente c)
    {
        comando.Parameters.AddWithValue("@nombres", c.Nombres);
        comando.Parameters.AddWithValue("@apellidos", c.Apellidos);
        comando.Parameters.AddWithValue("@identificacion", c.Identificacion);
        comando.Parameters.AddWithValue("@correo", c.Correo);
        comando.Parameters.AddWithValue("@telefono", c.Telefono);
        comando.Parameters.AddWithValue("@direccion", c.Direccion);
        comando.Parameters.AddWithValue("@ciudad", c.Ciudad);
        comando.Parameters.AddWithValue("@provincia", c.Provincia);
        comando.Parameters.AddWithValue("@fecha_nacimiento", c.FechaNacimiento.ToDateTime(TimeOnly.MinValue));
        comando.Parameters.AddWithValue("@estado", c.Estado);
    }

    private static Cliente LeerCliente(MySqlDataReader lector)
    {
        return new Cliente
        {
            Id = lector.GetInt32(0),
            Nombres = lector.GetString(1),
            Apellidos = lector.GetString(2),
            Identificacion = lector.GetString(3),
            Correo = lector.GetString(4),
            Telefono = lector.GetString(5),
            Direccion = lector.GetString(6),
            Ciudad = lector.GetString(7),
            Provincia = lector.GetString(8),
            FechaNacimiento = DateOnly.FromDateTime(lector.GetDateTime(9)),
            Estado = lector.GetString(10)
        };
    }
}
