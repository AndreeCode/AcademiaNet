using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using System.ComponentModel.DataAnnotations;

namespace Academic.Pages.Admin;

[Authorize(Roles = "Admin")]
public class CreateCoordinadorModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<CreateCoordinadorModel> _logger;

    public CreateCoordinadorModel(UserManager<IdentityUser> userManager, ILogger<CreateCoordinadorModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
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

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
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
            var existingUser = await _userManager.FindByEmailAsync(Input.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Input.Email", "Ya existe un usuario con ese email.");
                return Page();
            }

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

            await _userManager.AddToRoleAsync(user, "Coordinador");

            _logger.LogInformation("Coordinador creado: {Email}", user.Email);
            StatusMessage = $"Coordinador '{Input.Nombre} {Input.Apellido}' creado exitosamente. Email: {user.Email}";
            
            return RedirectToPage("/Admin/Dashboard");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear coordinador");
            StatusMessage = $"Error: {ex.Message}";
            return Page();
        }
    }
}
