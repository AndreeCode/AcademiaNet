using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;
using System.Security.Claims;

namespace Academic.Pages.Tutor;

[Authorize(Roles = "Tutor")]
public class AprobarMatriculasModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly ILogger<AprobarMatriculasModel> _logger;

    public AprobarMatriculasModel(AcademicContext context, ILogger<AprobarMatriculasModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public List<Matricula> MatriculasPendientes { get; set; } = new();
    public SelectList Salones { get; set; } = default!;
    public Models.Tutor? TutorActual { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostAprobarAsync(int matriculaId, int salonId)
    {
        try
        {
            var matricula = await _context.Matriculas
                .Include(m => m.Alumno)
                .Include(m => m.Ciclo)
                .FirstOrDefaultAsync(m => m.Id == matriculaId);

            if (matricula == null)
            {
                ErrorMessage = "Matrícula no encontrada.";
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

            // Asignar salón al alumno
            if (matricula.Alumno != null)
            {
                matricula.Alumno.SalonId = salonId;
            }

            // Actualizar estado de matrícula
            matricula.EstadoPago = EstadoPago.Pagado;
            matricula.FechaPago = DateTime.UtcNow;

            // Decrementar vacantes si aún no se ha hecho
            if (matricula.Ciclo != null && matricula.Ciclo.Vacantes > 0)
            {
                matricula.Ciclo.Vacantes--;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Tutor {Email} aprobó matrícula {MatriculaId}", userEmail, matriculaId);

            SuccessMessage = $"Matrícula de {matricula.Alumno?.Nombre} {matricula.Alumno?.Apellido} aprobada y asignado al salón {salon.Nombre}.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aprobar matrícula");
            ErrorMessage = "Error al aprobar la matrícula: " + ex.Message;
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostRechazarAsync(int matriculaId, string motivo)
    {
        try
        {
            var matricula = await _context.Matriculas
                .Include(m => m.Alumno)
                .FirstOrDefaultAsync(m => m.Id == matriculaId);

            if (matricula == null)
            {
                ErrorMessage = "Matrícula no encontrada.";
                return RedirectToPage();
            }

            matricula.EstadoPago = EstadoPago.Rechazado;
            matricula.Observaciones = $"Rechazada por tutor: {motivo}";

            await _context.SaveChangesAsync();

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            _logger.LogInformation("Tutor {Email} rechazó matrícula {MatriculaId}", userEmail, matriculaId);

            SuccessMessage = $"Matrícula de {matricula.Alumno?.Nombre} {matricula.Alumno?.Apellido} rechazada.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al rechazar matrícula");
            ErrorMessage = "Error al rechazar la matrícula: " + ex.Message;
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
            MatriculasPendientes = new List<Matricula>();
            Salones = new SelectList(Enumerable.Empty<Salon>());
            return;
        }

        MatriculasPendientes = await _context.Matriculas
            .Include(m => m.Alumno)
            .ThenInclude(a => a!.Salon)
            .ThenInclude(s => s!.Sede)
            .Include(m => m.Ciclo)
            .Where(m => m.EstadoPago == EstadoPago.Pendiente)
            .OrderByDescending(m => m.Id)
            .ToListAsync();

        var salonesDelTutor = TutorActual.TutorSalones
            .Where(ts => ts.Salon != null)
            .Select(ts => ts.Salon!)
            .ToList();

        Salones = new SelectList(
            salonesDelTutor.Select(s => new { 
                s.Id, 
                Nombre = $"{s.Nombre} - {s.Sede?.Nombre}" 
            }),
            "Id",
            "Nombre"
        );
    }
}
