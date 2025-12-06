namespace Academic.Models;

public sealed class Curso
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public int? ProfesorId { get; set; }
    public Profesor? Profesor { get; set; }
}
