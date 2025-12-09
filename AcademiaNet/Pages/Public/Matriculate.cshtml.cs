using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Services;
using Academic.Models;
using Microsoft.AspNetCore.Identity;

namespace Academic.Pages.Public;

public sealed class MatriculateModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly MercadoPagoService _mpService;
    private readonly UserManager<IdentityUser> _userManager;

    public MatriculateModel(AcademicContext context, MercadoPagoService mpService, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _mpService = mpService;
        _userManager = userManager;
    }

    public class InputModel
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool AcceptTerms { get; set; }
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public Ciclo? CurrentCiclo { get; set; }
    public bool MatriculaAbierta { get; set; }
    public string? MatriculaMensaje { get; set; }

    public async Task OnGetAsync()
    {
        CurrentCiclo = await _context.Ciclos.OrderByDescending(c => c.Id).FirstOrDefaultAsync();
        EvaluateMatriculaWindow();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        CurrentCiclo = await _context.Ciclos.OrderByDescending(c => c.Id).FirstOrDefaultAsync();
        EvaluateMatriculaWindow();

        if (!Input.AcceptTerms)
        {
            ModelState.AddModelError(string.Empty, "Debe aceptar los términos.");
            return Page();
        }

        if (CurrentCiclo is null)
        {
            ModelState.AddModelError(string.Empty, "No hay ciclo activo para matricular.");
            return Page();
        }

        // Require admin to set explicit matricula window
        if (!CurrentCiclo.MatriculaInicio.HasValue || !CurrentCiclo.MatriculaFin.HasValue)
        {
            ModelState.AddModelError(string.Empty, "La administración no ha definido el periodo de matrícula para este ciclo.");
            return Page();
        }

        var now = DateTime.UtcNow;
        var start = CurrentCiclo.MatriculaInicio.Value;
        var end = CurrentCiclo.MatriculaFin.Value;

        if (now < start)
        {
            ModelState.AddModelError(string.Empty, $"La matrícula aún no ha comenzado. Comienza: {start.ToString("g")}");
            return Page();
        }

        if (now > end)
        {
            ModelState.AddModelError(string.Empty, "La fecha de matrícula ha finalizado.");
            return Page();
        }

        // Vacantes check: only apply for presencial modalidad
        if (CurrentCiclo.Modalidad == ModalidadCiclo.Presencial && CurrentCiclo.Vacantes > 0)
        {
            var count = await _context.Matriculas.CountAsync(m => m.CicloId == CurrentCiclo.Id);
            if (count >= CurrentCiclo.Vacantes)
            {
                ModelState.AddModelError(string.Empty, "No hay vacantes disponibles para modalidad presencial.");
                return Page();
            }
        }

        // Prevent duplicate alumno by email
        var existingAlumno = await _context.Alumnos.FirstOrDefaultAsync(a => a.Email == Input.Email);
        if (existingAlumno != null)
        {
            ModelState.AddModelError(string.Empty, "Ya existe un alumno registrado con ese correo.");
            return Page();
        }

        var alumno = new Academic.Models.Alumno
        {
            Nombre = Input.Nombre,
            Apellido = Input.Apellido,
            Email = Input.Email
        };

        _context.Alumnos.Add(alumno);
        await _context.SaveChangesAsync();

        // Ensure Identity user for alumno exists and has role
        var identityUser = await _userManager.FindByEmailAsync(Input.Email);
        if (identityUser == null)
        {
            identityUser = new IdentityUser { Email = Input.Email, UserName = Input.Email, EmailConfirmed = true };
            var pwd = "Alumno123!"; // TODO: replace with secure random & email notification
            var create = await _userManager.CreateAsync(identityUser, pwd);
            if (create.Succeeded)
            {
                await _userManager.AddToRoleAsync(identityUser, "Alumno");
            }
            else
            {
                // log and continue; student domain exists but no identity account
                // In production we'd notify admin
            }
        }

        var matricula = new Academic.Models.Matricula
        {
            AlumnoId = alumno.Id,
            CicloId = CurrentCiclo.Id,
            Monto = 1.00m,
            Moneda = "PEN",
            EstadoPago = Academic.Models.EstadoPago.Pendiente,
            CreatedAt = DateTime.UtcNow
        };

        _context.Matriculas.Add(matricula);
        await _context.SaveChangesAsync();

        var host = $"{Request.Scheme}://{Request.Host}";
        var returnUrl = $"{host}/Public/Matriculate?mpStatus=return";

        var (initPoint, prefId) = await _mpService.CreatePreferenceAsync(matricula, returnUrl);
        if (initPoint is null)
        {
            TempData["Error"] = "Error creando preferencia de pago.";
            return Page();
        }

        matricula.MercadoPagoInitPoint = initPoint;
        matricula.MercadoPagoPreferenceId = prefId;
        await _context.SaveChangesAsync();

        return Redirect(initPoint);
    }

    private void EvaluateMatriculaWindow()
    {
        MatriculaAbierta = false;
        MatriculaMensaje = null;
        if (CurrentCiclo == null) return;

        if (!CurrentCiclo.MatriculaInicio.HasValue || !CurrentCiclo.MatriculaFin.HasValue)
        {
            MatriculaMensaje = "La administración no ha establecido fechas de matrícula para este ciclo.";
            return;
        }

        var now = DateTime.UtcNow;
        if (now < CurrentCiclo.MatriculaInicio.Value)
        {
            MatriculaMensaje = $"La matrícula empezará el {CurrentCiclo.MatriculaInicio.Value.ToString("g")}.";
            return;
        }
        if (now > CurrentCiclo.MatriculaFin.Value)
        {
            MatriculaMensaje = "La matrícula ha finalizado.";
            return;
        }

        // vacancy info
        if (CurrentCiclo.Modalidad == ModalidadCiclo.Presencial && CurrentCiclo.Vacantes > 0)
        {
            MatriculaMensaje = $"Modalidad: Presencial. Vacantes disponibles: {CurrentCiclo.Vacantes}.";
        }
        else if (CurrentCiclo.Modalidad == ModalidadCiclo.Virtual)
        {
            MatriculaMensaje = "Modalidad: Virtual. Inscripción sin límite de vacantes.";
        }
        else
        {
            MatriculaMensaje = $"Modalidad: {CurrentCiclo.Modalidad}. Vacantes: {(CurrentCiclo.Vacantes == 0 ? "Ilimitadas" : CurrentCiclo.Vacantes.ToString())}.";
        }

        MatriculaAbierta = true;
    }
}
