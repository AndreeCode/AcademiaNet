namespace Academic.Models;

public sealed class Profesor
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public List<Curso> Cursos { get; set; } = new();
}
