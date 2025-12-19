using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;
using Microsoft.AspNetCore.Identity;

namespace Academic.Pages.Admin;

[Authorize(Roles = "Admin")]
public class AsignarPadreModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<AsignarPadreModel> _logger;

    public AsignarPadreModel(
        AcademicContext context,
        UserManager<IdentityUser> userManager,
        ILogger<AsignarPadreModel> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public SelectList Alumnos { get; set; } = default!;
    public List<Models.Apoderado> ApoderadosActuales { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        public int AlumnoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string DNI { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public string? Parentesco { get; set; }
        public bool RecibeNotificaciones { get; set; } = true;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDataAsync();
            return Page();
        }

        try
        {
            var alumno = await _context.Alumnos.FindAsync(Input.AlumnoId);
            if (alumno == null)
            {
                ErrorMessage = "Alumno no encontrado.";
                await LoadDataAsync();
                return Page();
            }

            var apoderadoExistente = await _context.Apoderados
                .FirstOrDefaultAsync(a => a.Email == Input.Email);

            if (apoderadoExistente != null)
            {
                ErrorMessage = $"Ya existe un apoderado con el email {Input.Email}.";
                await LoadDataAsync();
                return Page();
            }

            var apoderado = new Models.Apoderado
            {
                AlumnoId = Input.AlumnoId,
                Nombre = Input.Nombre,
                Apellido = Input.Apellido,
                DNI = Input.DNI,
                Email = Input.Email,
                Telefono = Input.Telefono,
                Direccion = Input.Direccion,
                Parentesco = Input.Parentesco,
                RecibeNotificaciones = Input.RecibeNotificaciones,
                FechaRegistro = DateTime.UtcNow
            };

            _context.Apoderados.Add(apoderado);
            await _context.SaveChangesAsync();

            var identityUser = await _userManager.FindByEmailAsync(Input.Email);
            if (identityUser == null)
            {
                identityUser = new IdentityUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    EmailConfirmed = true
                };

                var defaultPassword = "Apoderado123!";
                var createResult = await _userManager.CreateAsync(identityUser, defaultPassword);

                if (createResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(identityUser, "Apoderado");
                    _logger.LogInformation("Usuario Identity creado para apoderado {Email}", Input.Email);
                }
                else
                {
                    _logger.LogWarning("No se pudo crear usuario Identity para {Email}: {Errors}",
                        Input.Email, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }

            _logger.LogInformation("Apoderado {ApoderadoId} asignado a alumno {AlumnoId}",
                apoderado.Id, Input.AlumnoId);

            SuccessMessage = $"Apoderado {Input.Nombre} {Input.Apellido} asignado exitosamente a {alumno.Nombre} {alumno.Apellido}.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al asignar apoderado");
            ErrorMessage = "Error al asignar apoderado: " + ex.Message;
            await LoadDataAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostEliminarAsync(int apoderadoId)
    {
        try
        {
            var apoderado = await _context.Apoderados.FindAsync(apoderadoId);
            if (apoderado == null)
            {
                ErrorMessage = "Apoderado no encontrado.";
                return RedirectToPage();
            }

            _context.Apoderados.Remove(apoderado);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Apoderado {ApoderadoId} eliminado", apoderadoId);

            SuccessMessage = "Apoderado eliminado exitosamente.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar apoderado");
            ErrorMessage = "Error al eliminar apoderado: " + ex.Message;
            return RedirectToPage();
        }
    }

    private async Task LoadDataAsync()
    {
        var alumnos = await _context.Alumnos
            .Where(a => a.IsActive)
            .OrderBy(a => a.Apellido)
            .ThenBy(a => a.Nombre)
            .Select(a => new
            {
                a.Id,
                NombreCompleto = a.Nombre + " " + a.Apellido + " - " + a.Email
            })
            .ToListAsync();

        Alumnos = new SelectList(alumnos, "Id", "NombreCompleto");

        ApoderadosActuales = await _context.Apoderados
            .Include(a => a.Alumno)
            .OrderByDescending(a => a.FechaRegistro)
            .ToListAsync();
    }
}
