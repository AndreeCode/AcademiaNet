using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;
using System.ComponentModel.DataAnnotations;
using TutorModel = Academic.Models.Tutor;

namespace Academic.Pages.Coordinador;

[Authorize(Roles = "Coordinador")]
public class DashboardModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<DashboardModel> _logger;

    public DashboardModel(
        AcademicContext context,
        UserManager<IdentityUser> userManager,
        ILogger<DashboardModel> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    // Estadísticas generales
    public int TotalAlumnos { get; set; }
    public int TotalProfesores { get; set; }
    public int TotalTutores { get; set; }
    public int TotalSalones { get; set; }
    public int MatriculasPendientes { get; set; }
    public int MatriculasAprobadas { get; set; }

    // Datos
    public Ciclo? CicloActual { get; set; }
    public List<Matricula> UltimasMatriculas { get; set; } = new();
    public List<Matricula> MatriculasPendientesLista { get; set; } = new();
    public List<Salon> Salones { get; set; } = new();
    public List<Profesor> Profesores { get; set; } = new();
    public List<TutorModel> Tutores { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        // Cargar ciclo actual
        CicloActual = await _context.Ciclos
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync();

        // Estadísticas
        TotalAlumnos = await _context.Alumnos.CountAsync(a => a.IsActive);
        TotalProfesores = await _context.Profesores.CountAsync();
        TotalTutores = await _context.Tutores.CountAsync(t => t.IsActive);
        TotalSalones = await _context.Salones.CountAsync();

        if (CicloActual != null)
        {
            MatriculasPendientes = await _context.Matriculas
                .Where(m => m.CicloId == CicloActual.Id && m.EstadoPago == EstadoPago.Pendiente)
                .CountAsync();

            MatriculasAprobadas = await _context.Matriculas
                .Where(m => m.CicloId == CicloActual.Id && m.EstadoPago == EstadoPago.Pagado)
                .CountAsync();

            // Últimas matrículas
            UltimasMatriculas = await _context.Matriculas
                .Where(m => m.CicloId == CicloActual.Id)
                .Include(m => m.Alumno)
                .OrderByDescending(m => m.CreatedAt)
                .Take(10)
                .ToListAsync();

            // Matrículas pendientes
            MatriculasPendientesLista = await _context.Matriculas
                .Where(m => m.CicloId == CicloActual.Id && m.EstadoPago == EstadoPago.Pendiente)
                .Include(m => m.Alumno)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        // Cargar salones con información
        Salones = await _context.Salones
            .Include(s => s.Sede)
            .Include(s => s.Profesor)
            .Include(s => s.Alumnos)
            .Include(s => s.TutorSalones)
                .ThenInclude(ts => ts.Tutor)
            .ToListAsync();

        // Cargar profesores
        Profesores = await _context.Profesores.ToListAsync();

        // Cargar tutores
        Tutores = await _context.Tutores.Where(t => t.IsActive).ToListAsync();
    }

    public async Task<IActionResult> OnPostAprobarMatriculaAsync(int matriculaId)
    {
        var matricula = await _context.Matriculas
            .Include(m => m.Alumno)
            .FirstOrDefaultAsync(m => m.Id == matriculaId);

        if (matricula == null)
        {
            StatusMessage = "Matrícula no encontrada.";
            return RedirectToPage();
        }

        matricula.EstadoPago = EstadoPago.Pagado;
        matricula.FechaPago = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Coordinador aprobó matrícula {MatriculaId} para alumno {Alumno}", 
            matriculaId, matricula.Alumno.Email);
        StatusMessage = $"Matrícula de {matricula.Alumno.Nombre} {matricula.Alumno.Apellido} aprobada.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRechazarMatriculaAsync(int matriculaId)
    {
        var matricula = await _context.Matriculas
            .Include(m => m.Alumno)
            .FirstOrDefaultAsync(m => m.Id == matriculaId);

        if (matricula == null)
        {
            StatusMessage = "Matrícula no encontrada.";
            return RedirectToPage();
        }

        matricula.EstadoPago = EstadoPago.Cancelado;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Coordinador rechazó matrícula {MatriculaId} para alumno {Alumno}", 
            matriculaId, matricula.Alumno.Email);
        StatusMessage = $"Matrícula de {matricula.Alumno.Nombre} {matricula.Alumno.Apellido} rechazada.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAsignarTutorSalonAsync(int tutorId, int salonId)
    {
        // Verificar si ya existe la asignación
        var existe = await _context.TutorSalones
            .AnyAsync(ts => ts.TutorId == tutorId && ts.SalonId == salonId);

        if (existe)
        {
            StatusMessage = "El tutor ya está asignado a ese salón.";
            return RedirectToPage();
        }

        var tutorSalon = new TutorSalon
        {
            TutorId = tutorId,
            SalonId = salonId
        };

        _context.TutorSalones.Add(tutorSalon);
        await _context.SaveChangesAsync();

        StatusMessage = "Tutor asignado al salón exitosamente.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoverTutorSalonAsync(int tutorId, int salonId)
    {
        var tutorSalon = await _context.TutorSalones
            .FirstOrDefaultAsync(ts => ts.TutorId == tutorId && ts.SalonId == salonId);

        if (tutorSalon != null)
        {
            _context.TutorSalones.Remove(tutorSalon);
            await _context.SaveChangesAsync();
            StatusMessage = "Tutor removido del salón.";
        }

        return RedirectToPage();
    }
}
