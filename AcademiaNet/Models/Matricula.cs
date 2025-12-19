namespace Academic.Models;

public enum EstadoPago
{
    Pendiente = 0,
    Pagado = 1,
    Cancelado = 2,
    Rechazado = 3
}

public enum TipoPasarela
{
    SinPasarela = 0,    // Matrícula manual (requiere aprobación de Admin/Tutor)
    MercadoPago = 1,    // Pago automático vía MercadoPago
    Culqi = 2           // Pago automático vía Culqi
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

    // Tipo de pasarela utilizada
    public TipoPasarela TipoPasarela { get; set; } = TipoPasarela.SinPasarela;

    // MercadoPago fields
    public string? MercadoPagoInitPoint { get; set; }
    public string? MercadoPagoPreferenceId { get; set; }
    public string? MercadoPagoPaymentId { get; set; }

    // Culqi fields
    public string? CulqiChargeId { get; set; }
    public string? CulqiTokenId { get; set; }
    public string? CulqiOrderId { get; set; }
    
    /// <summary>
    /// Fecha en que se realizó el pago
    /// </summary>
    public DateTime? FechaPago { get; set; }
    
    /// <summary>
    /// Monto pagado efectivamente
    /// </summary>
    public decimal? PaidAmount { get; set; }
    
    /// <summary>
    /// Fecha de creación de la matrícula
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Observaciones sobre la matrícula (motivo de rechazo, notas, etc.)
    /// </summary>
    public string? Observaciones { get; set; }
}
