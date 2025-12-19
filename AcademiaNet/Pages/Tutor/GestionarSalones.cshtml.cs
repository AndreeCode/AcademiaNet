using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Academic.Pages.Tutor;

[Authorize(Roles = "Tutor")]
public class GestionarSalonesModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly ILogger<GestionarSalonesModel> _logger;

    public GestionarSalonesModel(AcademicContext context, ILogger<GestionarSalonesModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public class SalonInputModel
    {
        [Required] [StringLength(50)] public string Nombre { get; set; } = string.Empty;
        [Required] public int SedeId { get; set; }
    }

    [BindProperty]
    public SalonInputModel Input { get; set; } = new();

    public List<Salon> MisSalones { get; set; } = new();
    public SelectList SedesSelectList { get; set; } = new SelectList(Enumerable.Empty<Sede>());
    public Models.Tutor? TutorActual { get; set; }

    [TempData] public string? SuccessMessage { get; set; }
    [TempData] public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDataAsync();
            return Page();
        }

        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var tutor = await _context.Tutores.FirstOrDefaultAsync(t => t.Email == userEmail);

            if (tutor == null)
            {
                ErrorMessage = "No se encontró el tutor asociado.";
                await LoadDataAsync();
                return Page();
            }

            var salon = new Salon
            {
                Nombre = Input.Nombre,
                SedeId = Input.SedeId
            };

            _context.Salones.Add(salon);
            await _context.SaveChangesAsync();

            _context.TutorSalones.Add(new TutorSalon
            {
                TutorId = tutor.Id,
                SalonId = salon.Id
            });

            await _context.SaveChangesAsync();

            SuccessMessage = $"Salón {salon.Nombre} creado y asignado exitosamente.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear salón");
            ErrorMessage = "Error al crear el salón: " + ex.Message;
            await LoadDataAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDesasignarAsync(int salonId)
    {
        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var tutor = await _context.Tutores.FirstOrDefaultAsync(t => t.Email == userEmail);

            if (tutor == null)
            {
                ErrorMessage = "Tutor no encontrado.";
                return RedirectToPage();
            }

            var tutorSalon = await _context.TutorSalones
                .FirstOrDefaultAsync(ts => ts.TutorId == tutor.Id && ts.SalonId == salonId);

            if (tutorSalon == null)
            {
                ErrorMessage = "Asignación no encontrada.";
                return RedirectToPage();
            }

            _context.TutorSalones.Remove(tutorSalon);
            await _context.SaveChangesAsync();

            SuccessMessage = "Salón desasignado correctamente.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desasignar salón");
            ErrorMessage = "Error al desasignar.";
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

        if (TutorActual != null)
        {
            MisSalones = TutorActual.TutorSalones
                .Select(ts => ts.Salon!)
                .OrderBy(s => s.Nombre)
                .ToList();
        }

        var sedes = await _context.Sedes.OrderBy(s => s.Nombre).ToListAsync();
        SedesSelectList = new SelectList(sedes, "Id", "Nombre");
    }
}
