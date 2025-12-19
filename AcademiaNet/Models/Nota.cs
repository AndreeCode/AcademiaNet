namespace Academic.Models;

/// <summary>
/// Representa una nota de un alumno en una materia o evaluación
/// </summary>
public sealed class Nota
{
    public int Id { get; set; }

    /// <summary>
    /// Alumno al que pertenece la nota
    /// </summary>
    public int AlumnoId { get; set; }
    public Alumno? Alumno { get; set; }

    /// <summary>
    /// Ciclo académico al que pertenece la nota
    /// </summary>
    public int CicloId { get; set; }
    public Ciclo? Ciclo { get; set; }

    /// <summary>
    /// Salón del alumno (opcional, para contexto)
    /// </summary>
    public int? SalonId { get; set; }
    public Salon? Salon { get; set; }

    /// <summary>
    /// Nombre de la materia o evaluación (ej: "Matemáticas", "Examen Parcial")
    /// </summary>
    public string Materia { get; set; } = string.Empty;

    /// <summary>
    /// Descripción adicional de la evaluación
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Nota obtenida (0-20 para sistema vigesimal peruano)
    /// </summary>
    public decimal Calificacion { get; set; }

    /// <summary>
    /// Peso de la nota para el cálculo del promedio (ej: 0.3 para 30%)
    /// </summary>
    public decimal Peso { get; set; } = 1.0m;

    /// <summary>
    /// Tipo de evaluación
    /// </summary>
    public TipoEvaluacion TipoEvaluacion { get; set; } = TipoEvaluacion.Practica;

    /// <summary>
    /// Fecha de la evaluación
    /// </summary>
    public DateTime FechaEvaluacion { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Usuario que registró la nota (email)
    /// </summary>
    public string? RegistradoPor { get; set; }

    /// <summary>
    /// Fecha de registro de la nota
    /// </summary>
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Observaciones adicionales
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Estado de la nota (activa o anulada)
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Tipo de evaluación
/// </summary>
public enum TipoEvaluacion
{
    Practica = 0,
    Tarea = 1,
    ExamenParcial = 2,
    ExamenFinal = 3,
    Proyecto = 4,
    Exposicion = 5,
    Participacion = 6,
    Otro = 7
}
