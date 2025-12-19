using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;
using AlumnoModel = Academic.Models.Alumno;

namespace Academic.Pages.Admin;

[Authorize(Roles = "Admin,Tutor,Coordinador")]
public class GestionarNotasModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly ILogger<GestionarNotasModel> _logger;

    public GestionarNotasModel(AcademicContext context, ILogger<GestionarNotasModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    [BindProperty]
    public Nota Nota { get; set; } = new();

    public List<AlumnoModel> Alumnos { get; set; } = new();
    public List<Ciclo> Ciclos { get; set; } = new();
    public List<Salon> Salones { get; set; } = new();
    public List<Nota> NotasRegistradas { get; set; } = new();
    
    public AlumnoModel? AlumnoSeleccionado { get; set; }
    public decimal? PromedioCalculado { get; set; }

    public string? MensajeExito { get; set; }
    public string? MensajeError { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? AlumnoIdFiltro { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? CicloIdFiltro { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await CargarDatosAsync();
        
        if (AlumnoIdFiltro.HasValue)
        {
            AlumnoSeleccionado = await _context.Alumnos
                .Include(a => a.Salon)
                .FirstOrDefaultAsync(a => a.Id == AlumnoIdFiltro.Value);

            NotasRegistradas = await _context.Notas
                .Include(n => n.Ciclo)
                .Include(n => n.Salon)
                .Where(n => n.AlumnoId == AlumnoIdFiltro.Value && n.IsActive)
                .OrderByDescending(n => n.FechaEvaluacion)
                .ToListAsync();

            if (CicloIdFiltro.HasValue)
            {
                NotasRegistradas = NotasRegistradas
                    .Where(n => n.CicloId == CicloIdFiltro.Value)
                    .ToList();
            }

            // Calcular promedio
            if (NotasRegistradas.Any())
            {
                var sumaCalificaciones = NotasRegistradas.Sum(n => n.Calificacion * n.Peso);
                var sumaPesos = NotasRegistradas.Sum(n => n.Peso);
                PromedioCalculado = sumaPesos > 0 ? sumaCalificaciones / sumaPesos : 0;
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostRegistrarNotaAsync()
    {
        if (!ModelState.IsValid)
        {
            await CargarDatosAsync();
            return Page();
        }

        try
        {
            // Validar que la calificación esté en rango válido (0-20)
            if (Nota.Calificacion < 0 || Nota.Calificacion > 20)
            {
                MensajeError = "La calificación debe estar entre 0 y 20.";
                await CargarDatosAsync();
                return Page();
            }

            Nota.RegistradoPor = User.Identity?.Name;
            Nota.FechaRegistro = DateTime.UtcNow;
            Nota.IsActive = true;

            _context.Notas.Add(Nota);
            await _context.SaveChangesAsync();

            // Recalcular promedio del alumno
            await RecalcularPromedioAlumnoAsync(Nota.AlumnoId, Nota.CicloId);

            _logger.LogInformation("Nota registrada para alumno {AlumnoId} por {Usuario}", 
                Nota.AlumnoId, User.Identity?.Name);

            MensajeExito = "? Nota registrada exitosamente. Promedio actualizado.";
            
            return RedirectToPage(new { AlumnoIdFiltro = Nota.AlumnoId, CicloIdFiltro = Nota.CicloId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar nota");
            MensajeError = "? Error al registrar la nota.";
            await CargarDatosAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostEliminarNotaAsync(int notaId)
    {
        try
        {
            var nota = await _context.Notas.FindAsync(notaId);
            if (nota == null)
            {
                MensajeError = "Nota no encontrada.";
                return RedirectToPage();
            }

            var alumnoId = nota.AlumnoId;
            var cicloId = nota.CicloId;

            // Marcar como inactiva en lugar de eliminar
            nota.IsActive = false;
            await _context.SaveChangesAsync();

            // Recalcular promedio
            await RecalcularPromedioAlumnoAsync(alumnoId, cicloId);

            _logger.LogInformation("Nota {NotaId} eliminada por {Usuario}", notaId, User.Identity?.Name);

            MensajeExito = "Nota eliminada exitosamente.";
            return RedirectToPage(new { AlumnoIdFiltro = alumnoId, CicloIdFiltro = cicloId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar nota");
            MensajeError = "Error al eliminar la nota.";
            return RedirectToPage();
        }
    }

    private async Task CargarDatosAsync()
    {
        // Cargar alumnos activos
        var query = _context.Alumnos.Include(a => a.Salon).Where(a => a.IsActive);

        // Si es Tutor, filtrar por sus salones
        if (User.IsInRole("Tutor"))
        {
            var tutorEmail = User.Identity?.Name;
            var tutor = await _context.Tutores
                .Include(t => t.TutorSalones)
                .FirstOrDefaultAsync(t => t.Email == tutorEmail);

            if (tutor != null)
            {
                var salonIds = tutor.TutorSalones.Select(ts => ts.SalonId).ToList();
                query = query.Where(a => a.SalonId.HasValue && salonIds.Contains(a.SalonId.Value));
            }
        }

        Alumnos = await query.OrderBy(a => a.Apellido).ToListAsync();
        Ciclos = await _context.Ciclos.OrderByDescending(c => c.FechaInicio).ToListAsync();
        Salones = await _context.Salones.Include(s => s.Sede).ToListAsync();
    }

    private async Task RecalcularPromedioAlumnoAsync(int alumnoId, int cicloId)
    {
        var alumno = await _context.Alumnos.FindAsync(alumnoId);
        if (alumno == null) return;

        var notasActivas = await _context.Notas
            .Where(n => n.AlumnoId == alumnoId && n.CicloId == cicloId && n.IsActive)
            .ToListAsync();

        if (notasActivas.Any())
        {
            var sumaCalificaciones = notasActivas.Sum(n => n.Calificacion * n.Peso);
            var sumaPesos = notasActivas.Sum(n => n.Peso);
            alumno.PromedioGeneral = sumaPesos > 0 ? sumaCalificaciones / sumaPesos : 0;
        }
        else
        {
            alumno.PromedioGeneral = null;
        }

        await _context.SaveChangesAsync();
    }
}
