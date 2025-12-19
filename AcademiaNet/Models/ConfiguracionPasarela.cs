namespace Academic.Models;

/// <summary>
/// Configuración global de la pasarela de pago activa
/// </summary>
public sealed class ConfiguracionPasarela
{
    public int Id { get; set; }
    
    /// <summary>
    /// Tipo de pasarela activa en el sistema
    /// </summary>
    public TipoPasarela PasarelaActiva { get; set; } = TipoPasarela.SinPasarela;
    
    /// <summary>
    /// Fecha de última modificación
    /// </summary>
    public DateTime UltimaModificacion { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Usuario que realizó el último cambio
    /// </summary>
    public string? ModificadoPor { get; set; }
}
