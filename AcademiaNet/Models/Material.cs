namespace Academic.Models;

public enum TipoMaterial
{
    PDF = 0,
    Video = 1,
    Documento = 2,
    Enlace = 3,
    Otro = 4
}

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

    public int? SemanaId { get; set; }
    public Semana? Semana { get; set; }

    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public TipoMaterial TipoMaterial { get; set; } = TipoMaterial.Documento;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // link to salon and tutor who owns/created material
    public int? SalonId { get; set; }
    public Salon? Salon { get; set; }
    public int? TutorId { get; set; }
    public Tutor? Tutor { get; set; }
    public int? CreatedById { get; set; }
}
