using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Academic.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public LoginModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public class InputModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public void OnGet(string? returnUrl = null)
    {
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, isPersistent: false, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user is not null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin")) return LocalRedirect("/Admin/Dashboard");
                if (roles.Contains("Profesor")) return LocalRedirect("/Profesor/Dashboard");
                if (roles.Contains("Tutor")) return LocalRedirect("/Tutor/Dashboard");
                if (roles.Contains("Coordinador")) return LocalRedirect("/Coordinador/Dashboard");
                return LocalRedirect("/Alumno/Dashboard");
            }

            return LocalRedirect(returnUrl ?? "/");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Cuenta temporalmente bloqueada. Intenta más tarde.");
            return Page();
        }

        if (result.IsNotAllowed)
        {
            ModelState.AddModelError(string.Empty, "Inicio de sesión no permitido. Verifica tu cuenta.");
            return Page();
        }

        ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
        return Page();
    }
}
