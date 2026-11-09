namespace GestionClientesSemana3;

// La clase Cliente representa una fila de la tabla clientes.
public class Cliente
{
    public int Id { get; set; }
    public string Nombres { get; set; } = "";
    public string Apellidos { get; set; } = "";
    public string Identificacion { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Direccion { get; set; } = "";
    public string Ciudad { get; set; } = "";
    public string Provincia { get; set; } = "";
    public DateOnly FechaNacimiento { get; set; }
    public string Estado { get; set; } = "Activo";
}
