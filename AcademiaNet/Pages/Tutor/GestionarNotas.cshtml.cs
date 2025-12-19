using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;
using AlumnoModel = Academic.Models.Alumno;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Academic.Pages.Tutor;

[Authorize(Roles = "Tutor")]
public class GestionarNotasModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly ILogger<GestionarNotasModel> _logger;

    public GestionarNotasModel(AcademicContext context, ILogger<GestionarNotasModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public class NotaInputModel
    {
        [Required] public int AlumnoId { get; set; }
        [Required] public int SalonId { get; set; }
        [Required] public int CicloId { get; set; }
        [Required] [StringLength(200)] public string Materia { get; set; } = string.Empty;
        [StringLength(500)] public string? Descripcion { get; set; }
        [Required] [Range(0, 20)] public decimal Calificacion { get; set; }
        [Range(0.1, 10)] public decimal Peso { get; set; } = 1.0m;
        public TipoEvaluacion TipoEvaluacion { get; set; }
        [Required] public DateTime FechaEvaluacion { get; set; } = DateTime.Now;
        public string? Observaciones { get; set; }
    }

    [BindProperty]
    public NotaInputModel Input { get; set; } = new();

    public List<Nota> Notas { get; set; } = new();
    public SelectList SalonesSelectList { get; set; } = new SelectList(Enumerable.Empty<Salon>());
    public SelectList AlumnosSelectList { get; set; } = new SelectList(Enumerable.Empty<AlumnoModel>());
    public SelectList CiclosSelectList { get; set; } = new SelectList(Enumerable.Empty<Ciclo>());

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
                ErrorMessage = "No se encontró el tutor asociado a su cuenta.";
                await LoadDataAsync();
                return Page();
            }

            // Verificar que el tutor tenga acceso al salón
            var tieneSalon = await _context.TutorSalones
                .AnyAsync(ts => ts.TutorId == tutor.Id && ts.SalonId == Input.SalonId);

            if (!tieneSalon)
            {
                ErrorMessage = "No tiene permisos para agregar notas a este salón.";
                await LoadDataAsync();
                return Page();
            }

            var nota = new Nota
            {
                AlumnoId = Input.AlumnoId,
                SalonId = Input.SalonId,
                CicloId = Input.CicloId,
                Materia = Input.Materia,
                Descripcion = Input.Descripcion,
                Calificacion = Input.Calificacion,
                Peso = Input.Peso,
                TipoEvaluacion = Input.TipoEvaluacion,
                FechaEvaluacion = Input.FechaEvaluacion,
                RegistradoPor = userEmail,
                FechaRegistro = DateTime.UtcNow,
                Observaciones = Input.Observaciones,
                IsActive = true
            };

            _context.Notas.Add(nota);
            await _context.SaveChangesAsync();

            // Recalcular promedio del alumno
            await RecalcularPromedioAsync(Input.AlumnoId);

            SuccessMessage = $"Nota registrada exitosamente para {nota.Materia}.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear nota");
            ErrorMessage = "Error al registrar la nota: " + ex.Message;
            await LoadDataAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            var nota = await _context.Notas.FindAsync(id);
            if (nota == null)
            {
                ErrorMessage = "Nota no encontrada.";
                return RedirectToPage();
            }

            var alumnoId = nota.AlumnoId;
            _context.Notas.Remove(nota);
            await _context.SaveChangesAsync();

            await RecalcularPromedioAsync(alumnoId);

            SuccessMessage = "Nota eliminada correctamente.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar nota");
            ErrorMessage = "Error al eliminar la nota.";
            return RedirectToPage();
        }
    }

    private async Task LoadDataAsync()
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        var tutor = await _context.Tutores
            .Include(t => t.TutorSalones)
            .ThenInclude(ts => ts.Salon)
            .FirstOrDefaultAsync(t => t.Email == userEmail);

        if (tutor == null) return;

        var salones = tutor.TutorSalones.Select(ts => ts.Salon).ToList();
        SalonesSelectList = new SelectList(
            salones.Select(s => new { s!.Id, Nombre = $"{s.Nombre} - {s.Sede?.Nombre}" }),
            "Id",
            "Nombre"
        );

        var salonIds = salones.Select(s => s!.Id).ToList();
        var alumnos = await _context.Alumnos
            .Where(a => a.SalonId.HasValue && salonIds.Contains(a.SalonId.Value) && a.IsActive)
            .OrderBy(a => a.Apellido)
            .ToListAsync();

        AlumnosSelectList = new SelectList(
            alumnos.Select(a => new { a.Id, Nombre = $"{a.Apellido}, {a.Nombre}" }),
            "Id",
            "Nombre"
        );

        var ciclos = await _context.Ciclos
            .OrderByDescending(c => c.Id)
            .ToListAsync();

        CiclosSelectList = new SelectList(ciclos, "Id", "Nombre");

        Notas = await _context.Notas
            .Include(n => n.Alumno)
            .Include(n => n.Salon)
            .Include(n => n.Ciclo)
            .Where(n => salonIds.Contains(n.SalonId ?? 0))
            .OrderByDescending(n => n.FechaRegistro)
            .ToListAsync();
    }

    private async Task RecalcularPromedioAsync(int alumnoId)
    {
        var notas = await _context.Notas
            .Where(n => n.AlumnoId == alumnoId && n.IsActive)
            .ToListAsync();

        if (notas.Any())
        {
            var promedio = notas.Sum(n => n.Calificacion * n.Peso) / notas.Sum(n => n.Peso);
            var alumno = await _context.Alumnos.FindAsync(alumnoId);
            if (alumno != null)
            {
                alumno.PromedioGeneral = promedio;
                await _context.SaveChangesAsync();
            }
        }
    }
}
