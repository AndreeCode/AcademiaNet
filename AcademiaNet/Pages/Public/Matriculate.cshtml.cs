using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Services;
using Academic.Models;

namespace Academic.Pages.Public;

public sealed class MatriculateModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly MercadoPagoService _mpService;

    public MatriculateModel(AcademicContext context, MercadoPagoService mpService)
    {
        _context = context;
        _mpService = mpService;
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

    public async Task OnGetAsync()
    {
        CurrentCiclo = await _context.Ciclos.OrderByDescending(c => c.Id).FirstOrDefaultAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        CurrentCiclo = await _context.Ciclos.OrderByDescending(c => c.Id).FirstOrDefaultAsync();

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

        var now = DateTime.UtcNow;
        var start = CurrentCiclo.MatriculaInicio ?? CurrentCiclo.FechaInicio;
        var end = CurrentCiclo.MatriculaFin ?? CurrentCiclo.FechaFin;

        if (now < start || now > end)
        {
            ModelState.AddModelError(string.Empty, "La matriculación no está abierta para el ciclo actual.");
            return Page();
        }

        // Vacantes check
        if (CurrentCiclo.Vacantes > 0)
        {
            var count = await _context.Matriculas.CountAsync(m => m.CicloId == CurrentCiclo.Id);
            if (count >= CurrentCiclo.Vacantes)
            {
                ModelState.AddModelError(string.Empty, "Las vacantes para este ciclo están llenas.");
                return Page();
            }
        }

        var alumno = new Academic.Models.Alumno
        {
            Nombre = Input.Nombre,
            Apellido = Input.Apellido,
            Email = Input.Email
        };

        _context.Alumnos.Add(alumno);
        await _context.SaveChangesAsync();

        var matricula = new Academic.Models.Matricula
        {
            AlumnoId = alumno.Id,
            CicloId = CurrentCiclo.Id,
            Monto = 1.00m,
            Moneda = "PEN",
            EstadoPago = "Pendiente",
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
}
