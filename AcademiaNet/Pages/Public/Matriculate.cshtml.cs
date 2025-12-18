using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Services;
using Academic.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace Academic.Pages.Public;

public sealed class MatriculateModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly MercadoPagoService _mpService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<MatriculateModel> _logger;
    private readonly IConfiguration _configuration;

    public MatriculateModel(
        AcademicContext context, 
        MercadoPagoService mpService, 
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ILogger<MatriculateModel> logger,
        IConfiguration configuration)
    {
        _context = context;
        _mpService = mpService;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _configuration = configuration;
    }

    public class InputModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es requerido")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "El DNI debe tener entre 8 y 20 caracteres")]
        [RegularExpression(@"^[A-Z0-9]{8,20}$", ErrorMessage = "El DNI debe contener solo letras mayúsculas y números (8-20 caracteres)")]
        [Display(Name = "DNI")]
        public string DNI { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar la contraseña")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        [Display(Name = "Teléfono (Opcional)")]
        public string? Telefono { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        [Display(Name = "Dirección (Opcional)")]
        public string? Direccion { get; set; }

        [Required(ErrorMessage = "Debe aceptar los términos y condiciones")]
        public bool AcceptTerms { get; set; }
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public Ciclo? CurrentCiclo { get; set; }
    public bool MatriculaAbierta { get; set; }
    public string? MatriculaMensaje { get; set; }
    public bool MercadoPagoEnabled { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        CurrentCiclo = await _context.Ciclos.OrderByDescending(c => c.Id).FirstOrDefaultAsync();
        MercadoPagoEnabled = _configuration.GetValue<bool>("MercadoPago:Enabled");
        EvaluateMatriculaWindow();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadCurrentCycleAsync();
            return Page();
        }

        // Validar términos
        if (!Input.AcceptTerms)
        {
            ModelState.AddModelError(string.Empty, "Debes aceptar los términos y condiciones.");
            await LoadCurrentCycleAsync();
            return Page();
        }

        await LoadCurrentCycleAsync();

        if (CurrentCiclo == null)
        {
            ErrorMessage = "No hay un ciclo disponible para matrícula.";
            return Page();
        }

        // Validar periodo de matrícula
        if (!MatriculaAbierta)
        {
            ErrorMessage = "El periodo de matrícula no está abierto.";
            return Page();
        }

        // Validar vacantes
        if (CurrentCiclo.Vacantes > 0)
        {
            var matriculasActuales = await _context.Matriculas
                .Where(m => m.CicloId == CurrentCiclo.Id && m.EstadoPago == Academic.Models.EstadoPago.Pagado)
                .CountAsync();

            if (matriculasActuales >= CurrentCiclo.Vacantes)
            {
                ErrorMessage = "Lo sentimos, ya no hay vacantes disponibles para este ciclo.";
                return Page();
            }
        }

        try
        {
            // Validar email único
            var existingUserByEmail = await _userManager.FindByEmailAsync(Input.Email);
            if (existingUserByEmail != null)
            {
                ErrorMessage = $"El email {Input.Email} ya está registrado. Si ya tienes cuenta, inicia sesión.";
                return Page();
            }

            // Validar DNI único
            var existingAlumnoDNI = await _context.Alumnos
                .FirstOrDefaultAsync(a => a.DNI == Input.DNI.ToUpper());

            if (existingAlumnoDNI != null)
            {
                ErrorMessage = $"El DNI {Input.DNI} ya está registrado en el sistema.";
                return Page();
            }

            // Verificar si Mercado Pago está habilitado
            var mercadoPagoEnabled = _configuration.GetValue<bool>("MercadoPago:Enabled");

            if (mercadoPagoEnabled)
            {
                // FLUJO CON MERCADO PAGO: Guardar datos en TempData y redirigir a MP
                TempData["PendingRegistration"] = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Input.Nombre,
                    Input.Apellido,
                    Input.DNI,
                    Input.Email,
                    Input.Password,
                    Input.Telefono,
                    Input.Direccion,
                    CicloId = CurrentCiclo.Id,
                    MontoMatricula = CurrentCiclo.MontoMatricula
                });

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                _logger.LogInformation("Creando preferencia de MP. BaseUrl: {BaseUrl}", baseUrl);

                var tempAlumno = new Academic.Models.Alumno
                {
                    Nombre = Input.Nombre,
                    Apellido = Input.Apellido,
                    Email = Input.Email,
                    Telefono = Input.Telefono
                };

                var tempMatricula = new Academic.Models.Matricula
                {
                    Id = 0,
                    Monto = CurrentCiclo.MontoMatricula,
                    Moneda = "PEN"
                };

                var (initPoint, preferenceId) = await _mpService.CreatePreferenceAsync(tempMatricula, tempAlumno, CurrentCiclo, baseUrl);

                if (string.IsNullOrWhiteSpace(initPoint))
                {
                    _logger.LogError("Mercado Pago no retornó initPoint.");
                    ErrorMessage = "Error al procesar el pago con Mercado Pago. Por favor, intente nuevamente más tarde o contacte con la administración.";
                    return Page();
                }

                TempData["MercadoPagoPreferenceId"] = preferenceId;
                _logger.LogInformation("Redirigiendo a MP. InitPoint: {InitPoint}", initPoint);

                return Redirect(initPoint);
            }
            else
            {
                // FLUJO SIN MERCADO PAGO: Crear usuario y matrícula inmediatamente con estado "Pendiente"
                _logger.LogInformation("Mercado Pago deshabilitado. Creando matrícula pendiente de aprobación manual.");

                // Crear alumno
                var alumno = new Academic.Models.Alumno
                {
                    Nombre = Input.Nombre,
                    Apellido = Input.Apellido,
                    DNI = Input.DNI.ToUpper(),
                    Email = Input.Email,
                    Telefono = Input.Telefono,
                    Direccion = Input.Direccion,
                    IsActive = true,
                    SalonId = null
                };

                _context.Alumnos.Add(alumno);
                await _context.SaveChangesAsync();

                // Crear matrícula en estado Pendiente
                var matricula = new Matricula
                {
                    AlumnoId = alumno.Id,
                    CicloId = CurrentCiclo.Id,
                    Monto = CurrentCiclo.MontoMatricula,
                    Moneda = "PEN",
                    EstadoPago = EstadoPago.Pendiente,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Matriculas.Add(matricula);
                await _context.SaveChangesAsync();

                // Crear usuario Identity
                var identityUser = new IdentityUser
                {
                    Email = Input.Email,
                    UserName = Input.Email,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(identityUser, Input.Password);

                if (!createResult.Succeeded)
                {
                    // Rollback: eliminar alumno y matrícula
                    _context.Matriculas.Remove(matricula);
                    _context.Alumnos.Remove(alumno);
                    await _context.SaveChangesAsync();

                    ErrorMessage = "Error al crear la cuenta de usuario: " + string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return Page();
                }

                // Asignar rol
                await _userManager.AddToRoleAsync(identityUser, "Alumno");

                // Login automático
                await _signInManager.SignInAsync(identityUser, isPersistent: false);

                SuccessMessage = $"¡Registro exitoso! Tu matrícula está pendiente de aprobación. Un tutor revisará tu pago y actualizará el estado.";
                
                return RedirectToPage("/Alumno/Dashboard");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar la matrícula");
            ErrorMessage = $"Error al procesar la matrícula: {ex.Message}. Por favor, intente nuevamente o contacte con la administración.";
            return Page();
        }
    }

    private void EvaluateMatriculaWindow()
    {
        MatriculaAbierta = false;
        MatriculaMensaje = null;
        if (CurrentCiclo == null) 
        {
            MatriculaMensaje = "No hay ciclo activo en este momento.";
            return;
        }

        if (!CurrentCiclo.MatriculaInicio.HasValue || !CurrentCiclo.MatriculaFin.HasValue)
        {
            MatriculaMensaje = "La administración no ha establecido fechas de matrícula para este ciclo.";
            return;
        }

        var now = DateTime.UtcNow;
        if (now < CurrentCiclo.MatriculaInicio.Value)
        {
            MatriculaMensaje = $"La matrícula empezará el {CurrentCiclo.MatriculaInicio.Value.ToString("dd/MM/yyyy HH:mm")}.";
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
            var vacantesOcupadas = _context.Matriculas.Count(m => m.CicloId == CurrentCiclo.Id);
            var vacantesDisponibles = CurrentCiclo.Vacantes - vacantesOcupadas;
            MatriculaMensaje = $"Modalidad: Presencial. Vacantes disponibles: {vacantesDisponibles} de {CurrentCiclo.Vacantes}.";
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

    private async Task LoadCurrentCycleAsync()
    {
        CurrentCiclo = await _context.Ciclos.OrderByDescending(c => c.Id).FirstOrDefaultAsync();
        MercadoPagoEnabled = _configuration.GetValue<bool>("MercadoPago:Enabled");
        EvaluateMatriculaWindow();
    }
}
