namespace Academic.Models;

public sealed class Semana
{
    public int Id { get; set; }
    
    public int NumeroSemana { get; set; }
    
    public int CicloId { get; set; }
    public Ciclo? Ciclo { get; set; }
    
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    
    public string? Tema { get; set; }
    public string? Descripcion { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public List<Material> Materiales { get; set; } = new();
}
