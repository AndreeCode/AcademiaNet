namespace Academic.Models;

public sealed class TutorSalon
{
    public int TutorId { get; set; }
    public Tutor? Tutor { get; set; }

    public int SalonId { get; set; }
    public Salon? Salon { get; set; }
}
