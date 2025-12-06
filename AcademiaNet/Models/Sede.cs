namespace Academic.Models;

public sealed class Sede
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }

    public List<Salon> Salones { get; set; } = new();
}
