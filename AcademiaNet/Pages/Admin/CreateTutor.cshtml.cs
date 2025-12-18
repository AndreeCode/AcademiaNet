using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;
using System.ComponentModel.DataAnnotations;

namespace Academic.Pages.Admin;

[Authorize(Roles = "Admin")]
public class CreateTutorModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public CreateTutorModel(AcademicContext context, UserManager<IdentityUser> userManager)
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
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y máximo {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // Verificar si ya existe un tutor con ese email
            var existingTutor = await _context.Tutores.FirstOrDefaultAsync(t => t.Email == Input.Email);
            if (existingTutor != null)
            {
                ModelState.AddModelError("Input.Email", "Ya existe un tutor con ese email.");
                return Page();
            }

            // Verificar si ya existe un usuario con ese email
            var existingUser = await _userManager.FindByEmailAsync(Input.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Input.Email", "Ya existe un usuario con ese email.");
                return Page();
            }

            // Crear usuario Identity
            var user = new IdentityUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, Input.Password);
            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            // Asignar rol de Tutor
            await _userManager.AddToRoleAsync(user, "Tutor");

            // Crear entidad Tutor
            var tutor = new Academic.Models.Tutor
            {
                Nombre = Input.Nombre,
                Apellido = Input.Apellido,
                Email = Input.Email,
                IsActive = Input.IsActive
            };

            _context.Tutores.Add(tutor);
            await _context.SaveChangesAsync();

            StatusMessage = $"Tutor '{tutor.Nombre} {tutor.Apellido}' creado exitosamente. Email: {tutor.Email}, Contraseña: {Input.Password}";
            return RedirectToPage("/Admin/Dashboard");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al crear el tutor: {ex.Message}";
            return Page();
        }
    }
}
