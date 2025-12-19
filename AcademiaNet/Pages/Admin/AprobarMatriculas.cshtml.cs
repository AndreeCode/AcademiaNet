using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;
using System.Security.Claims;

namespace Academic.Pages.Admin;

[Authorize(Roles = "Admin,Coordinador,Tutor")]
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
    public SelectList Sedes { get; set; } = default!;

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

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            _logger.LogInformation("Matrícula {MatriculaId} aprobada por {User}", matriculaId, userEmail);

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
            matricula.Observaciones = $"Rechazada: {motivo}";

            await _context.SaveChangesAsync();

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            _logger.LogInformation("Matrícula {MatriculaId} rechazada por {User}", matriculaId, userEmail);

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
        MatriculasPendientes = await _context.Matriculas
            .Include(m => m.Alumno)
            .ThenInclude(a => a!.Salon)
            .ThenInclude(s => s!.Sede)
            .Include(m => m.Ciclo)
            .Where(m => m.EstadoPago == EstadoPago.Pendiente)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        var salones = await _context.Salones
            .Include(s => s.Sede)
            .OrderBy(s => s.Nombre)
            .ToListAsync();

        Salones = new SelectList(
            salones.Select(s => new { s.Id, Nombre = $"{s.Nombre} - {s.Sede?.Nombre}" }),
            "Id",
            "Nombre"
        );

        var sedes = await _context.Sedes.OrderBy(s => s.Nombre).ToListAsync();
        Sedes = new SelectList(sedes, "Id", "Nombre");
    }
}
