namespace Academic.Models;

public sealed class Alumno
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public int? SalonId { get; set; }
    public Salon? Salon { get; set; }

    public List<Matricula> Matriculas { get; set; } = new();
}
