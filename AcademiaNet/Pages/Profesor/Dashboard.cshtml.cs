using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Academic.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Academic.Models;
using System.ComponentModel.DataAnnotations;

namespace Academic.Pages.Profesor;

[Authorize(Roles = "Profesor")]
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

    public Academic.Models.Profesor? ProfesorModel { get; set; }
    public List<Salon> MisSalones { get; set; } = new();
    public List<Curso> MisCursos { get; set; } = new();
    public List<Horario> MisHorarios { get; set; } = new();
    public List<Material> MisMateriales { get; set; } = new();
    public Ciclo? CicloActual { get; set; }
    public int TotalAlumnos { get; set; }
    public List<Semana> SemanasDelCiclo { get; set; } = new();

    [BindProperty]
    public string NewMaterialTitle { get; set; } = string.Empty;
    [BindProperty]
    public string? NewMaterialDescription { get; set; }
    [BindProperty]
    public int NewMaterialWeek { get; set; } = 1;
    [BindProperty]
    public int? NewMaterialSalonId { get; set; }
    [BindProperty]
    public int? NewMaterialCursoId { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return;

        ProfesorModel = await _context.Profesores
            .Include(p => p.Cursos)
            .FirstOrDefaultAsync(p => p.Email == user.Email);
        
        if (ProfesorModel == null) return;

        // Cargar ciclo actual
        CicloActual = await _context.Ciclos
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync();

        // Cargar semanas del ciclo
        if (CicloActual != null)
        {
            SemanasDelCiclo = await _context.Semanas
                .Where(s => s.CicloId == CicloActual.Id)
                .OrderBy(s => s.NumeroSemana)
                .ToListAsync();
        }

        // Cargar salones del profesor
        MisSalones = await _context.Salones
            .Where(s => s.ProfesorId == ProfesorModel.Id)
            .Include(s => s.Sede)
            .Include(s => s.Alumnos)
            .Include(s => s.Horarios)
            .Include(s => s.TutorSalones)
                .ThenInclude(ts => ts.Tutor)
            .ToListAsync();

        // Contar total de alumnos
        TotalAlumnos = MisSalones.Sum(s => s.Alumnos?.Count ?? 0);

        // Cargar cursos
        MisCursos = await _context.Cursos
            .Where(c => c.ProfesorId == ProfesorModel.Id)
            .ToListAsync();

        // Cargar horarios
        MisHorarios = await _context.Horarios
            .Where(h => h.Salon != null && h.Salon.ProfesorId == ProfesorModel.Id)
            .Include(h => h.Salon)
                .ThenInclude(s => s.Sede)
            .OrderBy(h => h.Dia)
            .ThenBy(h => h.HoraInicio)
            .ToListAsync();

        // Cargar materiales del profesor
        MisMateriales = await _context.Materiales
            .Where(m => m.Salon != null && m.Salon.ProfesorId == ProfesorModel.Id)
            .Include(m => m.Salon)
            .Include(m => m.Curso)
            .Include(m => m.Semana)
            .OrderByDescending(m => m.CreatedAt)
            .Take(20)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostCrearMaterialAsync()
    {
        if (string.IsNullOrWhiteSpace(NewMaterialTitle))
        {
            StatusMessage = "El título es requerido.";
            await OnGetAsync();
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Forbid();

        var profesor = await _context.Profesores.FirstOrDefaultAsync(p => p.Email == user.Email);
        if (profesor == null)
        {
            StatusMessage = "No se encontró el perfil de profesor.";
            await OnGetAsync();
            return Page();
        }

        var ciclo = await _context.Ciclos.OrderByDescending(c => c.Id).FirstOrDefaultAsync();

        // Buscar semana correspondiente
        Semana? semana = null;
        if (ciclo != null && NewMaterialWeek > 0)
        {
            semana = await _context.Semanas
                .FirstOrDefaultAsync(s => s.CicloId == ciclo.Id && s.NumeroSemana == NewMaterialWeek);
        }

        var material = new Material
        {
            Title = NewMaterialTitle,
            Description = NewMaterialDescription ?? string.Empty,
            Week = NewMaterialWeek,
            SalonId = NewMaterialSalonId,
            CursoId = NewMaterialCursoId,
            SemanaId = semana?.Id,
            Ciclo = ciclo,
            CreatedAt = DateTime.UtcNow,
            TipoMaterial = TipoMaterial.Documento
        };

        _context.Materiales.Add(material);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Profesor {Profesor} creó material {Material}", profesor.Email, material.Title);
        StatusMessage = "Material creado exitosamente.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEliminarMaterialAsync(int materialId)
    {
        var material = await _context.Materiales.FindAsync(materialId);
        if (material == null)
        {
            StatusMessage = "Material no encontrado.";
            return RedirectToPage();
        }

        _context.Materiales.Remove(material);
        await _context.SaveChangesAsync();

        StatusMessage = "Material eliminado exitosamente.";
        return RedirectToPage();
    }
}
