using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;
using AlumnoModel = Academic.Models.Alumno;
using System.Security.Claims;

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

    public AlumnoModel CurrentAlumno { get; set; } = default!;
    public Matricula? MatriculaActual { get; set; }
    public List<Material> Materiales { get; set; } = new();
    public Models.Apoderado? ApoderadoInfo { get; set; }
    public List<Nota> NotasAlumno { get; set; } = new();
    public decimal PromedioGeneral { get; set; }
    public Salon? Salon => CurrentAlumno?.Salon;
    public Sede? Sede => CurrentAlumno?.Salon?.Sede;
    public List<Matricula> Matriculas { get; set; } = new();
    public List<Horario> Horarios { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync()
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email);

        CurrentAlumno = await _context.Alumnos
            .Include(a => a.Salon)
            .ThenInclude(s => s!.Sede)
            .FirstOrDefaultAsync(a => a.Email == userEmail);

        if (CurrentAlumno == null)
        {
            Response.Redirect("/Account/Login");
            return;
        }

        // Obtener la matrícula actual
        MatriculaActual = await _context.Matriculas
            .Where(m => m.AlumnoId == CurrentAlumno.Id)
            .OrderByDescending(m => m.Id)
            .FirstOrDefaultAsync();

        // Si el usuario no tiene salón asignado, mostrar materiales solo si matrícula está aprobada
        if (CurrentAlumno.SalonId.HasValue || (MatriculaActual != null && MatriculaActual.EstadoPago == EstadoPago.Pagado))
        {
            Materiales = await _context.Materiales
                .Where(m => m.SalonId == CurrentAlumno.SalonId)
                .ToListAsync();
        }

        // Cargar información del apoderado si existe
        ApoderadoInfo = await _context.Apoderados
            .FirstOrDefaultAsync(a => a.AlumnoId == CurrentAlumno.Id);

        // Cargar notas del alumno
        NotasAlumno = await _context.Notas
            .Include(n => n.Ciclo)
            .Include(n => n.Salon)
            .Where(n => n.AlumnoId == CurrentAlumno.Id && n.IsActive)
            .OrderByDescending(n => n.FechaEvaluacion)
            .ToListAsync();

        PromedioGeneral = CurrentAlumno.PromedioGeneral ?? 0;

        // Cargar matrículas
        Matriculas = await _context.Matriculas
            .Include(m => m.Ciclo)
            .Where(m => m.AlumnoId == CurrentAlumno.Id)
            .OrderByDescending(m => m.Id)
            .ToListAsync();

        // Cargar horarios del salón
        if (CurrentAlumno.SalonId.HasValue)
        {
            Horarios = await _context.Horarios
                .Where(h => h.SalonId == CurrentAlumno.SalonId)
                .OrderBy(h => h.Dia)
                .ThenBy(h => h.HoraInicio)
                .ToListAsync();
        }
    }
}
