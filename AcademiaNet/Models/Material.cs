namespace Academic.Models;

public sealed class Material
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Week { get; set; }

    // Optional links to course or ciclo
    public int? CursoId { get; set; }
    public Curso? Curso { get; set; }

    public int? CicloId { get; set; }
    public Ciclo? Ciclo { get; set; }

    public string? FileUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
