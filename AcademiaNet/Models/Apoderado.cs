namespace Academic.Models;

public class Apoderado
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string DNI { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Parentesco { get; set; } // Padre, Madre, Tutor, etc.
    public bool RecibeNotificaciones { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    
    // Relación con Alumnos
    public int AlumnoId { get; set; }
    public Alumno? Alumno { get; set; }
}
