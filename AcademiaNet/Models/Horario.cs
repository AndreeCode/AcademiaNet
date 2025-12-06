namespace Academic.Models;

public sealed class Horario
{
    public int Id { get; set; }
    public int SalonId { get; set; }
    public Salon? Salon { get; set; }

    public DayOfWeek Dia { get; set; }
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFin { get; set; }

    public string? LinkMeet { get; set; }
}
