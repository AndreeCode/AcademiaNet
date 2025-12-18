using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Academic.Data;
using Academic.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Academic.Pages.Alumno;

[Authorize(Roles = "Alumno")]
public class ConfiguracionModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public ConfiguracionModel(AcademicContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "DNI")]
        public string? DNI { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime? DateOfBirth { get; set; }

        [Phone]
        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [StringLength(200)]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [StringLength(150)]
        [Display(Name = "Nombre del Apoderado")]
        public string? NombreApoderado { get; set; }

        [Phone]
        [StringLength(20)]
        [Display(Name = "Teléfono del Apoderado")]
        public string? TelefonoApoderado { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var alumno = await _context.Alumnos
            .FirstOrDefaultAsync(a => a.Email == user.Email);

        if (alumno == null)
        {
            // Create alumno if it doesn't exist
            alumno = new Academic.Models.Alumno
            {
                Email = user.Email!,
                Nombre = user.UserName ?? "",
                Apellido = "",
                IsActive = true
            };
            _context.Alumnos.Add(alumno);
            await _context.SaveChangesAsync();
            StatusMessage = "Se ha creado tu perfil de estudiante. Por favor, completa tu información.";
        }

        Input = new InputModel
        {
            Nombre = alumno.Nombre,
            Apellido = alumno.Apellido,
            DNI = alumno.DNI,
            DateOfBirth = alumno.DateOfBirth,
            Telefono = alumno.Telefono,
            Direccion = alumno.Direccion,
            NombreApoderado = alumno.NombreApoderado,
            TelefonoApoderado = alumno.TelefonoApoderado
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var alumno = await _context.Alumnos
            .FirstOrDefaultAsync(a => a.Email == user.Email);

        if (alumno == null)
        {
            StatusMessage = "Error: No se encontró el perfil del estudiante.";
            return RedirectToPage();
        }

        alumno.Nombre = Input.Nombre;
        alumno.Apellido = Input.Apellido;
        alumno.DNI = Input.DNI;
        alumno.DateOfBirth = Input.DateOfBirth;
        alumno.Telefono = Input.Telefono;
        alumno.Direccion = Input.Direccion;
        alumno.NombreApoderado = Input.NombreApoderado;
        alumno.TelefonoApoderado = Input.TelefonoApoderado;

        try
        {
            await _context.SaveChangesAsync();
            StatusMessage = "Tu perfil ha sido actualizado exitosamente.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al actualizar el perfil: {ex.Message}";
        }

        return RedirectToPage();
    }
}
