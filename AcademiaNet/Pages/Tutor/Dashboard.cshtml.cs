using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Academic.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace Academic.Pages.Tutor;

[Authorize(Roles = "Tutor")]
public class DashboardModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public DashboardModel(AcademicContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public Academic.Models.Tutor? Tutor { get; set; }
    public List<Academic.Models.Salon> Salones { get; set; } = new();

    [BindProperty]
    public string NewStudentEmail { get; set; } = string.Empty;
    [BindProperty]
    public string NewStudentNombre { get; set; } = string.Empty;
    [BindProperty]
    public string NewStudentApellido { get; set; } = string.Empty;

    // Material creation properties
    [BindProperty]
    public string NewMaterialTitle { get; set; } = string.Empty;
    [BindProperty]
    public string NewMaterialDescription { get; set; } = string.Empty;
    [BindProperty]
    public int NewMaterialWeek { get; set; } = 1;

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return;

        // find tutor by email
        Tutor = await _context.Tutores.FirstOrDefaultAsync(t => t.Email == user.Email);
        if (Tutor == null)
        {
            // still try to load salones by matching Tutor email via TutorSalones join
            Salones = await _context.Salones
                .Where(s => s.TutorSalones.Any(ts => ts.Tutor != null && ts.Tutor.Email == user.Email))
                .Include(s => s.Sede)
                .Include(s => s.Alumnos)
                .Include(s => s.Horarios)
                .Include(s => s.Materiales)
                .ToListAsync();
            return;
        }

        // load salones by Tutor.Id but also include join by email as fallback
        Salones = await _context.Salones
            .Where(s => s.TutorSalones.Any(ts => ts.TutorId == Tutor.Id) || s.TutorSalones.Any(ts => ts.Tutor != null && ts.Tutor.Email == Tutor.Email))
            .Include(s => s.Sede)
            .Include(s => s.Alumnos)
            .Include(s => s.Horarios)
            .Include(s => s.Materiales)
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

        // create domain Alumno
        var alumno = new Academic.Models.Alumno { Nombre = NewStudentNombre, Apellido = NewStudentApellido, Email = NewStudentEmail, SalonId = salonId };
        _context.Alumnos.Add(alumno);
        await _context.SaveChangesAsync();

        // create Identity user for the student if not exists
        var existing = await _userManager.FindByEmailAsync(NewStudentEmail);
        if (existing == null)
        {
            var identityUser = new IdentityUser { UserName = NewStudentEmail, Email = NewStudentEmail, EmailConfirmed = true };
            var defaultPassword = "Alumno123!"; // development default

            var createResult = await _userManager.CreateAsync(identityUser, defaultPassword);
            if (!createResult.Succeeded)
            {
                // do not block domain creation, but inform in ModelState
                ModelState.AddModelError(string.Empty, "No se pudo crear la cuenta de acceso para el alumno. Revisa el email o la configuración de Identity.");
                await OnGetAsync();
                return Page();
            }

            // add role
            await _userManager.AddToRoleAsync(identityUser, "Alumno");
        }

        // assign matricula to current ciclo
        var ciclo = await _context.Ciclos.OrderByDescending(c => c.Id).FirstOrDefaultAsync();
        if (ciclo != null)
        {
            _context.Matriculas.Add(new Academic.Models.Matricula { Alumno = alumno, Ciclo = ciclo, Monto = 1m, Moneda = "PEN", EstadoPago = Academic.Models.EstadoPago.Pendiente, CreatedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateMaterialAsync(int salonId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Forbid();

        // ensure tutor loaded
        Tutor = await _context.Tutores.FirstOrDefaultAsync(t => t.Email == user.Email);
        if (Tutor == null)
        {
            // try to locate tutor by identity email and create minimal domain entry if missing
            var t = await _context.Tutores.FirstOrDefaultAsync(x => x.Email == user.Email);
            if (t == null)
            {
                // cannot create material without tutor domain entry
                ModelState.AddModelError(string.Empty, "No se encontró el perfil de tutor.");
                await OnGetAsync();
                return Page();
            }
            Tutor = t;
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

        // optional: ensure tutor is assigned to this salon
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

        return RedirectToPage();
    }
}
