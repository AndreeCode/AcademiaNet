namespace Academic.Models;

public sealed class Salon
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public int SedeId { get; set; }
    public Sede? Sede { get; set; }

    public List<TutorSalon> TutorSalones { get; set; } = new();
    public int? ProfesorId { get; set; }
    public Profesor? Profesor { get; set; }

    public List<Horario> Horarios { get; set; } = new();
    public List<Alumno> Alumnos { get; set; } = new();
    public List<Material> Materiales { get; set; } = new();
}
