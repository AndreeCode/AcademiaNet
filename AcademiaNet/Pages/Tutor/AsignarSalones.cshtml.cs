using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;
using AlumnoModel = Academic.Models.Alumno;
using System.Security.Claims;

namespace Academic.Pages.Tutor;

[Authorize(Roles = "Tutor")]
public class AsignarSalonesModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly ILogger<AsignarSalonesModel> _logger;

    public AsignarSalonesModel(AcademicContext context, ILogger<AsignarSalonesModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public List<AlumnoModel> AlumnosSinSalon { get; set; } = new();
    public SelectList SalonesDisponibles { get; set; } = default!;
    public Models.Tutor? TutorActual { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsignarAsync(int alumnoId, int salonId)
    {
        try
        {
            var alumno = await _context.Alumnos.FindAsync(alumnoId);
            if (alumno == null)
            {
                ErrorMessage = "Alumno no encontrado.";
                return RedirectToPage();
            }

            var salon = await _context.Salones
                .Include(s => s.Sede)
                .FirstOrDefaultAsync(s => s.Id == salonId);

            if (salon == null)
            {
                ErrorMessage = "Salón no encontrado.";
                return RedirectToPage();
            }

            // Verificar que el tutor tenga acceso a este salón
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var tutor = await _context.Tutores
                .Include(t => t.TutorSalones)
                .FirstOrDefaultAsync(t => t.Email == userEmail);

            if (tutor == null || !tutor.TutorSalones.Any(ts => ts.SalonId == salonId))
            {
                ErrorMessage = "No tienes permisos para asignar a este salón.";
                return RedirectToPage();
            }

            alumno.SalonId = salonId;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tutor {Email} asignó alumno {AlumnoId} al salón {SalonId}", 
                userEmail, alumnoId, salonId);

            SuccessMessage = $"Alumno {alumno.Nombre} {alumno.Apellido} asignado al salón {salon.Nombre} exitosamente.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al asignar salón");
            ErrorMessage = "Error al asignar salón: " + ex.Message;
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostRemoverAsync(int alumnoId)
    {
        try
        {
            var alumno = await _context.Alumnos.FindAsync(alumnoId);
            if (alumno == null)
            {
                ErrorMessage = "Alumno no encontrado.";
                return RedirectToPage();
            }

            alumno.SalonId = null;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Salón removido para alumno {AlumnoId}", alumnoId);

            SuccessMessage = $"Salón removido para {alumno.Nombre} {alumno.Apellido}.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al remover salón");
            ErrorMessage = "Error al remover salón: " + ex.Message;
            return RedirectToPage();
        }
    }

    private async Task LoadDataAsync()
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        
        TutorActual = await _context.Tutores
            .Include(t => t.TutorSalones)
            .ThenInclude(ts => ts.Salon)
            .ThenInclude(s => s!.Sede)
            .FirstOrDefaultAsync(t => t.Email == userEmail);

        if (TutorActual == null)
        {
            AlumnosSinSalon = new List<AlumnoModel>();
            SalonesDisponibles = new SelectList(Enumerable.Empty<Salon>());
            return;
        }

        // Obtener alumnos activos sin salón asignado
        AlumnosSinSalon = await _context.Alumnos
            .Where(a => a.IsActive && (a.SalonId == null || a.SalonId == 0))
            .OrderBy(a => a.Apellido)
            .ThenBy(a => a.Nombre)
            .ToListAsync();

        // Obtener salones del tutor
        var salonesDelTutor = TutorActual.TutorSalones
            .Where(ts => ts.Salon != null)
            .Select(ts => ts.Salon!)
            .ToList();

        SalonesDisponibles = new SelectList(
            salonesDelTutor.Select(s => new { 
                s.Id, 
                Nombre = $"{s.Nombre} - {s.Sede?.Nombre}" 
            }),
            "Id",
            "Nombre"
        );
    }
}
