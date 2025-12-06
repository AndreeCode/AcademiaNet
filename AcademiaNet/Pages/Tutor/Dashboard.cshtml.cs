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

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return;

        // find tutor by email
        Tutor = await _context.Tutores.FirstOrDefaultAsync(t => t.Email == user.Email);
        if (Tutor == null) return;

        Salones = await _context.Salones
            .Where(s => s.TutorSalones.Any(ts => ts.TutorId == Tutor.Id))
            .Include(s => s.Sede)
            .Include(s => s.Alumnos)
            .Include(s => s.Horarios)
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
            _context.Matriculas.Add(new Academic.Models.Matricula { Alumno = alumno, Ciclo = ciclo, Monto = 1m, Moneda = "PEN", EstadoPago = "Pendiente", CreatedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
        }

        return RedirectToPage();
    }
}
