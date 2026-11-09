using System.Net.Mail;
using System.Text.RegularExpressions;
using GestionClientesSemana3;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);
var cadena = builder.Configuration.GetConnectionString("Clientes")
    ?? throw new InvalidOperationException("Falta configurar ConnectionStrings:Clientes.");
builder.Services.AddSingleton(new ClienteRepositorio(cadena));

var app = builder.Build();


app.MapGet("/", () => Results.File(
    Path.Combine(app.Environment.ContentRootPath, "index.html"), "text/html; charset=utf-8"));
app.MapGet("/index.html", () => Results.File(
    Path.Combine(app.Environment.ContentRootPath, "index.html"), "text/html; charset=utf-8"));
app.MapGet("/estilos.css", () => Results.File(
    Path.Combine(app.Environment.ContentRootPath, "estilos.css"), "text/css; charset=utf-8"));
app.MapGet("/app.js", () => Results.File(
    Path.Combine(app.Environment.ContentRootPath, "app.js"), "text/javascript; charset=utf-8"));


app.MapGet("/api/clientes", async (ClienteRepositorio repositorio) =>
    Results.Ok(await repositorio.ListarAsync()));


app.MapGet("/api/clientes/{id:int}", async (int id, ClienteRepositorio repositorio) =>
{
    var cliente = await repositorio.ObtenerAsync(id);
    return cliente is null ? (IResult)Results.NotFound(new { mensaje = "Cliente no encontrado." })
                           : Results.Ok(cliente);
});

app.MapPost("/api/clientes", async (Cliente cliente, ClienteRepositorio repositorio) =>
{
    var error = Validar(cliente);
    if (error is not null) return (IResult)Results.BadRequest(new { mensaje = error });

    try
    {
        var id = await repositorio.CrearAsync(cliente);
        return (IResult)Results.Created($"/api/clientes/{id}", new { id, mensaje = "Cliente registrado correctamente." });
    }
    catch (MySqlException ex) when (ex.Number == 1062)
    {
        return (IResult)Results.Conflict(new { mensaje = "Ya existe un cliente con esa identificación." });
    }
});


app.MapPut("/api/clientes/{id:int}", async (int id, Cliente cliente, ClienteRepositorio repositorio) =>
{
    var error = Validar(cliente);
    if (error is not null) return (IResult)Results.BadRequest(new { mensaje = error });

    try
    {
        var actualizado = await repositorio.ActualizarAsync(id, cliente);
        return actualizado ? (IResult)Results.Ok(new { mensaje = "Cliente actualizado correctamente." })
                           : Results.NotFound(new { mensaje = "Cliente no encontrado." });
    }
    catch (MySqlException ex) when (ex.Number == 1062)
    {
        return (IResult)Results.Conflict(new { mensaje = "La identificación ya pertenece a otro cliente." });
    }
});


app.MapDelete("/api/clientes/{id:int}", async (int id, ClienteRepositorio repositorio) =>
{
    var eliminado = await repositorio.EliminarAsync(id);
    return eliminado ? (IResult)Results.Ok(new { mensaje = "Cliente eliminado correctamente." })
                     : Results.NotFound(new { mensaje = "Cliente no encontrado." });
});


app.Run("http://localhost:5070");

static string? Validar(Cliente c)
{
    c.Nombres = c.Nombres?.Trim() ?? "";
    c.Apellidos = c.Apellidos?.Trim() ?? "";
    c.Identificacion = c.Identificacion?.Trim() ?? "";
    c.Correo = c.Correo?.Trim() ?? "";
    c.Telefono = c.Telefono?.Trim() ?? "";
    c.Direccion = c.Direccion?.Trim() ?? "";
    c.Ciudad = c.Ciudad?.Trim() ?? "";
    c.Provincia = c.Provincia?.Trim() ?? "";
    c.Estado = c.Estado?.Trim() ?? "";

    if (new[] { c.Nombres, c.Apellidos, c.Identificacion, c.Correo,
                c.Telefono, c.Direccion, c.Ciudad, c.Provincia, c.Estado }
        .Any(string.IsNullOrWhiteSpace))
        return "Completa todos los campos del formulario.";

    if (c.Nombres.Length > 80 || c.Apellidos.Length > 80 || c.Ciudad.Length > 80 ||
        c.Provincia.Length > 80 || c.Correo.Length > 150 || c.Direccion.Length > 160)
        return "Uno de los textos excede el tamaño permitido.";

    if (!Regex.IsMatch(c.Identificacion, @"^[a-zA-Z0-9-]{5,20}$"))
        return "La identificación debe tener entre 5 y 20 letras o números.";

    if (!MailAddress.TryCreate(c.Correo, out _))
        return "Ingresa un correo electrónico válido.";

    if (!Regex.IsMatch(c.Telefono, @"^\+?[0-9\s-]{7,20}$"))
        return "El teléfono debe contener entre 7 y 20 caracteres numéricos válidos.";

    if (c.FechaNacimiento == default || c.FechaNacimiento > DateOnly.FromDateTime(DateTime.Today))
        return "Selecciona una fecha de nacimiento válida.";

    if (c.Estado is not ("Activo" or "Inactivo"))
        return "Selecciona un estado válido.";

    return null;
}
