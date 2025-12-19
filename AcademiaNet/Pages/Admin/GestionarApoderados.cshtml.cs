using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;
using AlumnoModel = Academic.Models.Alumno;
using ApoderadoModel = Academic.Models.Apoderado;
using System.ComponentModel.DataAnnotations;

namespace Academic.Pages.Admin;

[Authorize(Roles = "Admin,Coordinador")]
public class GestionarApoderadosModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly ILogger<GestionarApoderadosModel> _logger;

    public GestionarApoderadosModel(AcademicContext context, ILogger<GestionarApoderadosModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public class ApoderadoInputModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Seleccione un alumno")]
        public int AlumnoId { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es requerido")]
        [StringLength(20)]
        public string DNI { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es requerido")]
        [StringLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Direccion { get; set; }

        [StringLength(50)]
        public string? Parentesco { get; set; }

        public bool RecibeNotificaciones { get; set; } = true;
    }

    [BindProperty]
    public ApoderadoInputModel Input { get; set; } = new();

    public List<ApoderadoModel> Apoderados { get; set; } = new();
    public SelectList AlumnosSelectList { get; set; } = new SelectList(Enumerable.Empty<AlumnoModel>());

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

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
            var apoderado = new ApoderadoModel
            {
                AlumnoId = Input.AlumnoId,
                Nombre = Input.Nombre,
                Apellido = Input.Apellido,
                DNI = Input.DNI.ToUpper(),
                Email = Input.Email,
                Telefono = Input.Telefono,
                Direccion = Input.Direccion,
                Parentesco = Input.Parentesco,
                RecibeNotificaciones = Input.RecibeNotificaciones,
                FechaRegistro = DateTime.UtcNow
            };

            _context.Apoderados.Add(apoderado);
            await _context.SaveChangesAsync();

            SuccessMessage = $"Apoderado {apoderado.Nombre} {apoderado.Apellido} registrado exitosamente.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear apoderado");
            ErrorMessage = "Error al crear el apoderado: " + ex.Message;
            await LoadDataAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            var apoderado = await _context.Apoderados.FindAsync(id);
            if (apoderado == null)
            {
                ErrorMessage = "Apoderado no encontrado.";
                return RedirectToPage();
            }

            _context.Apoderados.Remove(apoderado);
            await _context.SaveChangesAsync();

            SuccessMessage = $"Apoderado {apoderado.Nombre} {apoderado.Apellido} eliminado.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar apoderado");
            ErrorMessage = "Error al eliminar el apoderado: " + ex.Message;
            return RedirectToPage();
        }
    }

    private async Task LoadDataAsync()
    {
        Apoderados = await _context.Apoderados
            .Include(a => a.Alumno)
            .OrderBy(a => a.Alumno!.Apellido)
            .ThenBy(a => a.Alumno!.Nombre)
            .ToListAsync();

        var alumnos = await _context.Alumnos
            .Where(a => a.IsActive)
            .OrderBy(a => a.Apellido)
            .ThenBy(a => a.Nombre)
            .ToListAsync();

        AlumnosSelectList = new SelectList(
            alumnos.Select(a => new
            {
                a.Id,
                NombreCompleto = $"{a.Apellido}, {a.Nombre} - DNI: {a.DNI}"
            }),
            "Id",
            "NombreCompleto"
        );
    }
}
