using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Services;
using Academic.Models;
using AlumnoModel = Academic.Models.Alumno;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace Academic.Pages.Public;

public sealed class MatriculateModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly MercadoPagoService _mpService;
    private readonly CulqiService _culqiService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<MatriculateModel> _logger;
    private readonly IConfiguration _configuration;

    public MatriculateModel(
        AcademicContext context, 
        MercadoPagoService mpService,
        CulqiService culqiService,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ILogger<MatriculateModel> logger,
        IConfiguration configuration)
    {
        _context = context;
        _mpService = mpService;
        _culqiService = culqiService;
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
    public TipoPasarela PasarelaActiva { get; set; }
    public string CulqiPublicKey { get; set; } = string.Empty;
    public int VacantesOcupadas { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        CurrentCiclo = await _context.Ciclos.OrderByDescending(c => c.Id).FirstOrDefaultAsync();
        
        // Calcular vacantes ocupadas
        if (CurrentCiclo != null)
        {
            VacantesOcupadas = await _context.Matriculas
                .Where(m => m.CicloId == CurrentCiclo.Id && m.EstadoPago == EstadoPago.Pagado)
                .CountAsync();
        }
        
        // Obtener configuración de pasarela activa
        var config = await _context.ConfiguracionPasarelas.FirstOrDefaultAsync();
        PasarelaActiva = config?.PasarelaActiva ?? TipoPasarela.Culqi; // Por defecto Culqi
        
        // Obtener Public Key de Culqi si está activa
        if (PasarelaActiva == TipoPasarela.Culqi)
        {
            CulqiPublicKey = _configuration.GetValue<string>("Culqi:PublicKey") ?? string.Empty;
        }
        
        EvaluateMatriculaWindow();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadCurrentCycleAsync();
            return Page();
        }

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

        if (!MatriculaAbierta)
        {
            ErrorMessage = "El periodo de matrícula no está abierto.";
            return Page();
        }

        // Validar vacantes
        if (CurrentCiclo.Vacantes > 0)
        {
            var matriculasActuales = await _context.Matriculas
                .Where(m => m.CicloId == CurrentCiclo.Id && m.EstadoPago == EstadoPago.Pagado)
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

            // Obtener configuración de pasarela activa
            var config = await _context.ConfiguracionPasarelas.FirstOrDefaultAsync();
            var pasarelaActiva = config?.PasarelaActiva ?? TipoPasarela.Culqi;

            switch (pasarelaActiva)
            {
                case TipoPasarela.Culqi:
                    return await ProcessCulqiMatriculaAsync();

                case TipoPasarela.MercadoPago:
                    return await ProcessMercadoPagoMatriculaAsync();

                case TipoPasarela.SinPasarela:
                default:
                    return await ProcessManualMatriculaAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar la matrícula");
            ErrorMessage = $"Error al procesar la matrícula: {ex.Message}";
            return Page();
        }
    }

    private async Task<IActionResult> ProcessCulqiMatriculaAsync()
    {
        _logger.LogInformation("Procesando matrícula con Culqi para {Email}", Input.Email);

        // Guardar datos temporales para después del pago
        TempData["PendingCulqiRegistration"] = System.Text.Json.JsonSerializer.Serialize(new
        {
            Input.Nombre,
            Input.Apellido,
            Input.DNI,
            Input.Email,
            Input.Password,
            Input.Telefono,
            Input.Direccion,
            CicloId = CurrentCiclo!.Id,
            MontoMatricula = CurrentCiclo.MontoMatricula
        });

        _logger.LogInformation("? Datos guardados en TempData. Frontend mostrará Culqi Checkout.");

        // NO redirigir - dejar que el frontend muestre Culqi Checkout
        // Recargar la página para que se mantenga el formulario
        await LoadCurrentCycleAsync();
        return Page();
    }

    private async Task<IActionResult> ProcessMercadoPagoMatriculaAsync()
    {
        _logger.LogInformation("Procesando matrícula con MercadoPago para {Email}", Input.Email);

        TempData["PendingRegistration"] = System.Text.Json.JsonSerializer.Serialize(new
        {
            Input.Nombre,
            Input.Apellido,
            Input.DNI,
            Input.Email,
            Input.Password,
            Input.Telefono,
            Input.Direccion,
            CicloId = CurrentCiclo!.Id,
            MontoMatricula = CurrentCiclo.MontoMatricula
        });

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var tempAlumno = new AlumnoModel
        {
            Nombre = Input.Nombre,
            Apellido = Input.Apellido,
            Email = Input.Email,
            Telefono = Input.Telefono
        };

        var tempMatricula = new Matricula
        {
            Id = 0,
            Monto = CurrentCiclo.MontoMatricula,
            Moneda = "PEN"
        };

        var result = await _mpService.CreatePreferenceAsync(
            tempMatricula, tempAlumno, CurrentCiclo, baseUrl);
        
        var initPoint = result.initPoint;
        var preferenceId = result.preferenceId;

        if (string.IsNullOrWhiteSpace(initPoint))
        {
            ErrorMessage = "Error al procesar el pago con MercadoPago.";
            return Page();
        }

        TempData["MercadoPagoPreferenceId"] = preferenceId;
        return Redirect(initPoint);
    }

    private async Task<IActionResult> ProcessManualMatriculaAsync()
    {
        _logger.LogInformation("Procesando matrícula manual (sin pasarela) para {Email}", Input.Email);

        // Crear alumno
        var alumno = new AlumnoModel
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
            CicloId = CurrentCiclo!.Id,
            Monto = CurrentCiclo.MontoMatricula,
            Moneda = "PEN",
            EstadoPago = EstadoPago.Pendiente,
            TipoPasarela = TipoPasarela.SinPasarela,
            CreatedAt = DateTime.UtcNow
        };

        _context.Matriculas.Add(matricula);
        await _context.SaveChangesAsync();

        // NO decrementar vacantes aquí - se hará al aprobar
        // Vacantes se decrementan solo cuando Admin/Coordinador/Tutor aprueba la matrícula

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
            _context.Matriculas.Remove(matricula);
            _context.Alumnos.Remove(alumno);
            await _context.SaveChangesAsync();

            ErrorMessage = "Error al crear la cuenta: " + string.Join(", ", 
                createResult.Errors.Select(e => e.Description));
            return Page();
        }

        await _userManager.AddToRoleAsync(identityUser, "Alumno");
        await _signInManager.SignInAsync(identityUser, isPersistent: false);

        SuccessMessage = "¡Registro exitoso! Tu matrícula está PENDIENTE DE APROBACIÓN. Un administrador verificará tu pago y te asignará un salón.";
        return RedirectToPage("/Alumno/Dashboard");
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
        
        // Calcular vacantes ocupadas
        if (CurrentCiclo != null)
        {
            VacantesOcupadas = await _context.Matriculas
                .Where(m => m.CicloId == CurrentCiclo.Id && m.EstadoPago == EstadoPago.Pagado)
                .CountAsync();
        }
        
        // Obtener configuración de pasarela activa
        var config = await _context.ConfiguracionPasarelas.FirstOrDefaultAsync();
        PasarelaActiva = config?.PasarelaActiva ?? TipoPasarela.Culqi;
        
        // Obtener Public Key de Culqi si está activa
        if (PasarelaActiva == TipoPasarela.Culqi)
        {
            CulqiPublicKey = _configuration.GetValue<string>("Culqi:PublicKey") ?? string.Empty;
        }
        
        EvaluateMatriculaWindow();
    }
}
