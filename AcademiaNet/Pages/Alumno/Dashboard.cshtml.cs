using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Academic.Data;
using Academic.Models;
using Microsoft.EntityFrameworkCore;

namespace Academic.Pages.Alumno;

[Authorize(Roles = "Alumno")]
public class DashboardModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public DashboardModel(AcademicContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public Academic.Models.Alumno? CurrentAlumno { get; set; }
    public List<Matricula> Matriculas { get; set; } = new();
    public List<Horario> Horarios { get; set; } = new();
    public Salon? Salon { get; set; }
    public Sede? Sede { get; set; }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return;

        // map by email
        CurrentAlumno = await _context.Alumnos
            .Include(a => a.Matriculas)
                .ThenInclude(m => m.Ciclo)
            .Include(a => a.Salon)
                .ThenInclude(s => s.Sede)
            .FirstOrDefaultAsync(a => a.Email == user.Email);

        if (CurrentAlumno is null) return;

        Matriculas = await _context.Matriculas
            .Where(m => m.AlumnoId == CurrentAlumno.Id)
            .Include(m => m.Ciclo)
            .ToListAsync();

        Salon = CurrentAlumno.Salon;
        Sede = Salon?.Sede;

        if (Salon != null)
        {
            Horarios = await _context.Horarios
                .Where(h => h.SalonId == Salon.Id)
                .ToListAsync();
        }
    }
}
