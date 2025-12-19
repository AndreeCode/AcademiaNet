using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;

namespace Academic.Pages.Public;

public class MatriculaResultModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public MatriculaResultModel(
        AcademicContext context,
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager)
    {
        _context = context;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public string Status { get; set; } = string.Empty;
    public Matricula? Matricula { get; set; }
    public Academic.Models.Alumno? Alumno { get; set; }
    public Ciclo? Ciclo { get; set; }
    public string Message { get; set; } = string.Empty;
    public string MessageType { get; set; } = "info"; // success, warning, danger, info

    public async Task<IActionResult> OnGetAsync(string status, int matriculaId, string? collection_id, string? collection_status, string? payment_id, string? payment_type, string? preference_id)
    {
        Status = status?.ToLowerInvariant() ?? "unknown";

        // Recuperar datos pendientes de TempData
        var pendingRegistrationJson = TempData["PendingRegistration"] as string;
        
        if (string.IsNullOrWhiteSpace(pendingRegistrationJson))
        {
            Message = "No se encontró información de registro pendiente. Por favor, intente matricularse nuevamente.";
            MessageType = "danger";
            return Page();
        }

        // Deserializar datos guardados
        var pendingData = System.Text.Json.JsonSerializer.Deserialize<PendingRegistrationData>(pendingRegistrationJson);
        
        if (pendingData == null)
        {
            Message = "Error al recuperar datos de registro. Por favor, intente nuevamente.";
            MessageType = "danger";
            return Page();
        }

        // Procesar según el estado del pago
        switch (Status)
        {
            case "success":
            case "approved":
                // PAGO EXITOSO - Ahora SÍ crear usuario y matrícula
                try
                {
                    // 1. Verificar que el email no exista (doble validación)
                    var existingUser = await _userManager.FindByEmailAsync(pendingData.Email);
                    if (existingUser != null)
                    {
                        Message = $"El usuario con email {pendingData.Email} ya existe. Por favor, inicia sesión.";
                        MessageType = "warning";
                        return Page();
                    }

                    // 2. Verificar DNI único
                    var existingAlumno = await _context.Alumnos.FirstOrDefaultAsync(a => a.DNI == pendingData.DNI);
                    if (existingAlumno != null)
                    {
                        Message = $"El alumno con DNI {pendingData.DNI} ya existe en el sistema.";
                        MessageType = "warning";
                        return Page();
                    }

                    // 3. Crear alumno
                    var alumno = new Academic.Models.Alumno
                    {
                        Nombre = pendingData.Nombre,
                        Apellido = pendingData.Apellido,
                        DNI = pendingData.DNI.ToUpper(),
                        Email = pendingData.Email,
                        Telefono = pendingData.Telefono,
                        Direccion = pendingData.Direccion,
                        IsActive = true,
                        SalonId = null
                    };

                    _context.Alumnos.Add(alumno);
                    await _context.SaveChangesAsync();

                    // 4. Crear matrícula
                    var ciclo = await _context.Ciclos.FindAsync(pendingData.CicloId);
                    if (ciclo == null)
                    {
                        Message = "Error: Ciclo no encontrado.";
                        MessageType = "danger";
                        return Page();
                    }

                    var matricula = new Matricula
                    {
                        AlumnoId = alumno.Id,
                        CicloId = pendingData.CicloId,
                        Monto = pendingData.MontoMatricula,
                        Moneda = "PEN",
                        EstadoPago = EstadoPago.Pagado,
                        FechaPago = DateTime.UtcNow,
                        MercadoPagoPaymentId = payment_id,
                        MercadoPagoPreferenceId = preference_id ?? TempData["MercadoPagoPreferenceId"] as string,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Matriculas.Add(matricula);
                    await _context.SaveChangesAsync();

                    // Decrementar vacantes
                    if (ciclo.Vacantes > 0)
                    {
                        ciclo.Vacantes--;
                        _context.Ciclos.Update(ciclo);
                        await _context.SaveChangesAsync();
                    }

                    // 5. Crear usuario Identity
                    var identityUser = new IdentityUser
                    {
                        Email = pendingData.Email,
                        UserName = pendingData.Email,
                        EmailConfirmed = true
                    };

                    var createResult = await _userManager.CreateAsync(identityUser, pendingData.Password);

                    if (!createResult.Succeeded)
                    {
                        Message = "Error al crear la cuenta de usuario: " + string.Join(", ", createResult.Errors.Select(e => e.Description));
                        MessageType = "danger";
                        return Page();
                    }

                    // 6. Asignar rol
                    await _userManager.AddToRoleAsync(identityUser, "Alumno");

                    // 7. Iniciar sesión automáticamente
                    await _signInManager.SignInAsync(identityUser, isPersistent: false);

                    // 8. Configurar datos para la vista
                    Matricula = matricula;
                    Alumno = alumno;
                    Ciclo = ciclo;

                    Message = $"¡Pago exitoso! Tu matrícula para {Ciclo.Nombre} ha sido confirmada. Bienvenido(a) a Academia Zoe.";
                    MessageType = "success";

                    return Page();
                }
                catch (Exception ex)
                {
                    Message = $"Error al completar el registro: {ex.Message}. Por favor, contacte con la administración con tu comprobante de pago.";
                    MessageType = "danger";
                    return Page();
                }

            case "pending":
                // Pago pendiente - Mostrar mensaje pero NO crear registro
                Message = $"Tu pago está pendiente de confirmación. Te notificaremos por email cuando sea procesado. No se ha completado tu registro aún.";
                MessageType = "warning";
                
                // Preservar TempData para cuando se confirme el pago
                TempData.Keep("PendingRegistration");
                TempData.Keep("MercadoPagoPreferenceId");
                break;

            case "failure":
            case "rejected":
                // Pago rechazado - NO crear nada
                Message = $"Tu pago fue rechazado. Esto puede deberse a fondos insuficientes o datos incorrectos. No se ha completado tu registro.";
                MessageType = "danger";
                
                // NO preservar TempData - el usuario deberá volver a intentar desde el inicio
                break;

            default:
                Message = "Estado de pago desconocido. Por favor, contacta con la administración.";
                MessageType = "info";
                break;
        }

        return Page();
    }

    // Clase auxiliar para deserializar datos
    private class PendingRegistrationData
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string DNI { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public int CicloId { get; set; }
        public decimal MontoMatricula { get; set; }
    }
}
