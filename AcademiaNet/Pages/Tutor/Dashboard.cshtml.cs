using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Academic.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Academic.Models;

namespace Academic.Pages.Tutor;

[Authorize(Roles = "Tutor")]
public class DashboardModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<DashboardModel> _logger;

    public DashboardModel(AcademicContext context, UserManager<IdentityUser> userManager, ILogger<DashboardModel> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public Academic.Models.Tutor? Tutor { get; set; }
    public List<Academic.Models.Salon> Salones { get; set; } = new();
    public List<Matricula> MatriculasPendientes { get; set; } = new();

    [BindProperty]
    public string NewStudentEmail { get; set; } = string.Empty;
    [BindProperty]
    public string NewStudentNombre { get; set; } = string.Empty;
    [BindProperty]
    public string NewStudentApellido { get; set; } = string.Empty;

    [BindProperty]
    public string NewMaterialTitle { get; set; } = string.Empty;
    [BindProperty]
    public string NewMaterialDescription { get; set; } = string.Empty;
    [BindProperty]
    public int NewMaterialWeek { get; set; } = 1;

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return;

        Tutor = await _context.Tutores.FirstOrDefaultAsync(t => t.Email == user.Email);
        if (Tutor == null)
        {
            Salones = await _context.Salones
                .Where(s => s.TutorSalones.Any(ts => ts.Tutor != null && ts.Tutor.Email == user.Email))
                .Include(s => s.Sede)
                .Include(s => s.Alumnos)
                .Include(s => s.Horarios)
                .Include(s => s.Materiales)
                .ToListAsync();
            return;
        }

        Salones = await _context.Salones
            .Where(s => s.TutorSalones.Any(ts => ts.TutorId == Tutor.Id) || s.TutorSalones.Any(ts => ts.Tutor != null && ts.Tutor.Email == Tutor.Email))
            .Include(s => s.Sede)
            .Include(s => s.Alumnos)
            .Include(s => s.Horarios)
            .Include(s => s.Materiales)
            .ToListAsync();

        // Cargar matrículas pendientes de los alumnos en los salones del tutor
        var salonIds = Salones.Select(s => s.Id).ToList();
        MatriculasPendientes = await _context.Matriculas
            .Include(m => m.Alumno)
            .Include(m => m.Ciclo)
            .Where(m => m.EstadoPago == EstadoPago.Pendiente && m.Alumno.SalonId.HasValue && salonIds.Contains(m.Alumno.SalonId.Value))
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostCreateStudentAsync(int salonId)
    {
        if (string.IsNullOrWhiteSpace(NewStudentEmail) || string.IsNullOrWhiteSpace(NewStudentNombre))
        {
            ModelState.AddModelError(string.Empty, "Nombre y email son requeridos.");
            await OnGetAsync();
            return Page();
        }

        var alumno = new Academic.Models.Alumno 
        { 
            Nombre = NewStudentNombre, 
            Apellido = NewStudentApellido, 
            Email = NewStudentEmail, 
            SalonId = salonId 
        };
        _context.Alumnos.Add(alumno);
        await _context.SaveChangesAsync();

        var existing = await _userManager.FindByEmailAsync(NewStudentEmail);
        if (existing == null)
        {
            var identityUser = new IdentityUser { UserName = NewStudentEmail, Email = NewStudentEmail, EmailConfirmed = true };
            var defaultPassword = "Alumno123!";

            var createResult = await _userManager.CreateAsync(identityUser, defaultPassword);
            if (!createResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "No se pudo crear la cuenta de acceso para el alumno.");
                await OnGetAsync();
                return Page();
            }

            await _userManager.AddToRoleAsync(identityUser, "Alumno");
        }

        var ciclo = await _context.Ciclos.OrderByDescending(c => c.Id).FirstOrDefaultAsync();
        if (ciclo != null)
        {
            _context.Matriculas.Add(new Academic.Models.Matricula 
            { 
                Alumno = alumno, 
                Ciclo = ciclo, 
                Monto = ciclo.MontoMatricula, 
                Moneda = "PEN", 
                EstadoPago = EstadoPago.Pendiente, 
                CreatedAt = DateTime.UtcNow 
            });
            await _context.SaveChangesAsync();
        }

        StatusMessage = $"Alumno {NewStudentNombre} creado exitosamente.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateMaterialAsync(int salonId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Forbid();

        Tutor = await _context.Tutores.FirstOrDefaultAsync(t => t.Email == user.Email);
        if (Tutor == null)
        {
            ModelState.AddModelError(string.Empty, "No se encontró el perfil de tutor.");
            await OnGetAsync();
            return Page();
        }

        if (string.IsNullOrWhiteSpace(NewMaterialTitle))
        {
            ModelState.AddModelError(string.Empty, "El título es requerido.");
            await OnGetAsync();
            return Page();
        }

        var salon = await _context.Salones.FirstOrDefaultAsync(s => s.Id == salonId);
        if (salon == null)
        {
            ModelState.AddModelError(string.Empty, "Salón no encontrado.");
            await OnGetAsync();
            return Page();
        }

        var isAssigned = await _context.TutorSalones.AnyAsync(ts => ts.SalonId == salonId && (ts.TutorId == Tutor.Id || (ts.Tutor != null && ts.Tutor.Email == Tutor.Email)));
        if (!isAssigned)
        {
            ModelState.AddModelError(string.Empty, "No estás asignado a este salón.");
            await OnGetAsync();
            return Page();
        }

        var ciclo = await _context.Ciclos.OrderByDescending(c => c.Id).FirstOrDefaultAsync();

        var material = new Academic.Models.Material
        {
            Title = NewMaterialTitle,
            Description = NewMaterialDescription ?? string.Empty,
            Week = NewMaterialWeek <= 0 ? 1 : NewMaterialWeek,
            SalonId = salonId,
            TutorId = Tutor.Id,
            Ciclo = ciclo,
            CreatedAt = DateTime.UtcNow
        };

        _context.Materiales.Add(material);
        await _context.SaveChangesAsync();

        StatusMessage = "Material creado exitosamente.";
        return RedirectToPage();
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

        _logger.LogInformation("Tutor aprobó matrícula {MatriculaId} para alumno {Alumno}", matriculaId, matricula.Alumno.Email);
        StatusMessage = $"Matrícula de {matricula.Alumno.Nombre} {matricula.Alumno.Apellido} aprobada exitosamente.";

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

        _logger.LogInformation("Tutor rechazó matrícula {MatriculaId} para alumno {Alumno}", matriculaId, matricula.Alumno.Email);
        StatusMessage = $"Matrícula de {matricula.Alumno.Nombre} {matricula.Alumno.Apellido} rechazada.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCambiarSalonAlumnoAsync(int alumnoId, int nuevoSalonId)
    {
        var alumno = await _context.Alumnos.FindAsync(alumnoId);
        if (alumno == null)
        {
            StatusMessage = "Alumno no encontrado.";
            return RedirectToPage();
        }

        var salon = await _context.Salones.FindAsync(nuevoSalonId);
        if (salon == null)
        {
            StatusMessage = "Salón no encontrado.";
            return RedirectToPage();
        }

        alumno.SalonId = nuevoSalonId;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Alumno {Alumno} cambiado al salón {Salon}", alumno.Email, salon.Nombre);
        StatusMessage = $"Alumno {alumno.Nombre} {alumno.Apellido} movido al salón {salon.Nombre}.";

        return RedirectToPage();
    }
}
