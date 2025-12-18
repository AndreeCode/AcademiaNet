namespace Academic.Models;

public enum EstadoPago
{
    Pendiente = 0,
    Pagado = 1,
    Cancelado = 2,
    Rechazado = 3
}

public sealed class Matricula
{
    public int Id { get; set; }

    public int AlumnoId { get; set; }
    public Alumno? Alumno { get; set; }

    public int CicloId { get; set; }
    public Ciclo? Ciclo { get; set; }

    public decimal Monto { get; set; }
    public string Moneda { get; set; } = "PEN";
    public EstadoPago EstadoPago { get; set; } = EstadoPago.Pendiente;

    public string? MercadoPagoInitPoint { get; set; }
    public string? MercadoPagoPreferenceId { get; set; }
    public string? MercadoPagoPaymentId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Payment details
    public DateTime? FechaPago { get; set; }
    public decimal? PaidAmount { get; set; }
}
