namespace Academic.Models;

public sealed class Ciclo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }

    // Number of vacancies allowed for enrollment (0 = unlimited)
    public int Vacantes { get; set; } = 0;

    // Optional enrollment window (if null, use FechaInicio..FechaFin)
    public DateTime? MatriculaInicio { get; set; }
    public DateTime? MatriculaFin { get; set; }

    public List<Matricula> Matriculas { get; set; } = new();
}
