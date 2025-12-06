namespace Academic.Models;

public sealed class Matricula
{
    public int Id { get; set; }

    public int AlumnoId { get; set; }
    public Alumno? Alumno { get; set; }

    public int CicloId { get; set; }
    public Ciclo? Ciclo { get; set; }

    public decimal Monto { get; set; }
    public string Moneda { get; set; } = "PEN";
    public string EstadoPago { get; set; } = "Pendiente";

    public string? MercadoPagoInitPoint { get; set; }
    public string? MercadoPagoPreferenceId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
