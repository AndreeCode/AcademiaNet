namespace Academic.Models;

public sealed class Alumno
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public int? SalonId { get; set; }
    public Salon? Salon { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? DateOfBirth { get; set; }
    
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? DNI { get; set; }
    public string? NombreApoderado { get; set; }
    public string? TelefonoApoderado { get; set; }

    // Computed age (not mapped to DB explicitly)
    public int? Age {
        get {
            if (!DateOfBirth.HasValue) return null;
            var today = DateTime.UtcNow.Date;
            var age = today.Year - DateOfBirth.Value.Date.Year;
            if (DateOfBirth.Value.Date > today.AddYears(-age)) age--;
            return age;
        }
    }

    public List<Matricula> Matriculas { get; set; } = new();
}
