using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;
using AlumnoModel = Academic.Models.Alumno;
using Academic.Services;
using System.Text.Json;

namespace Academic.Pages.Public;

public class CulqiCallbackModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly CulqiService _culqiService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<CulqiCallbackModel> _logger;

    public CulqiCallbackModel(
        AcademicContext context,
        CulqiService culqiService,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ILogger<CulqiCallbackModel> logger)
    {
        _context = context;
        _culqiService = culqiService;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public string? Mensaje { get; set; }
    public bool Exito { get; set; }

    public async Task<IActionResult> OnPostAsync([FromBody] CulqiTokenRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return BadRequest(new { error = "Token inválido" });
            }

            // Obtener datos de la matrícula pendiente
            var pendingData = TempData["PendingCulqiRegistration"] as string;
            if (string.IsNullOrWhiteSpace(pendingData))
            {
                _logger.LogError("No se encontraron datos de matrícula pendiente");
                return BadRequest(new { error = "No se encontraron datos de matrícula" });
            }

            var registrationData = JsonSerializer.Deserialize<PendingRegistrationData>(pendingData);
            if (registrationData == null)
            {
                return BadRequest(new { error = "Datos de matrícula inválidos" });
            }

            // Crear cargo en Culqi
            var (success, chargeId, error) = await _culqiService.CreateChargeAsync(
                tokenId: request.Token,
                amount: registrationData.MontoMatricula,
                email: registrationData.Email,
                description: $"Matrícula - Ciclo {registrationData.CicloId}",
                externalReference: $"MATRICULA-TEMP-{DateTime.UtcNow.Ticks}"
            );

            if (!success || string.IsNullOrWhiteSpace(chargeId))
            {
                _logger.LogError("Error al crear cargo en Culqi: {Error}", error);
                return BadRequest(new { error = error ?? "Error al procesar el pago" });
            }

            _logger.LogInformation("? Pago exitoso con Culqi. ChargeId: {ChargeId}", chargeId);

            // Crear alumno
            var alumno = new AlumnoModel
            {
                Nombre = registrationData.Nombre,
                Apellido = registrationData.Apellido,
                DNI = registrationData.DNI.ToUpper(),
                Email = registrationData.Email,
                Telefono = registrationData.Telefono,
                Direccion = registrationData.Direccion,
                IsActive = true,
                SalonId = null
            };

            _context.Alumnos.Add(alumno);
            await _context.SaveChangesAsync();

            // Crear matrícula con estado Pagado
            var matricula = new Matricula
            {
                AlumnoId = alumno.Id,
                CicloId = registrationData.CicloId,
                Monto = registrationData.MontoMatricula,
                Moneda = "PEN",
                EstadoPago = EstadoPago.Pagado,
                TipoPasarela = TipoPasarela.Culqi,
                CulqiChargeId = chargeId,
                CulqiTokenId = request.Token,
                FechaPago = DateTime.UtcNow,
                PaidAmount = registrationData.MontoMatricula,
                CreatedAt = DateTime.UtcNow
            };

            _context.Matriculas.Add(matricula);
            await _context.SaveChangesAsync();

            // Decrementar vacantes del ciclo
            var ciclo = await _context.Ciclos.FindAsync(registrationData.CicloId);
            if (ciclo != null && ciclo.Vacantes > 0)
            {
                ciclo.Vacantes--;
                _context.Ciclos.Update(ciclo);
                await _context.SaveChangesAsync();
                _logger.LogInformation("? Vacantes decrementadas. Quedan {Vacantes} vacantes", ciclo.Vacantes);
            }

            // Crear usuario Identity
            var identityUser = new IdentityUser
            {
                Email = registrationData.Email,
                UserName = registrationData.Email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(identityUser, registrationData.Password);

            if (!createResult.Succeeded)
            {
                _logger.LogError("Error al crear usuario Identity: {Errors}", 
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
                
                // Rollback
                _context.Matriculas.Remove(matricula);
                _context.Alumnos.Remove(alumno);
                await _context.SaveChangesAsync();

                return BadRequest(new { error = "Error al crear cuenta de usuario" });
            }

            await _userManager.AddToRoleAsync(identityUser, "Alumno");

            _logger.LogInformation("? Matrícula completada exitosamente para {Email}", registrationData.Email);

            return new JsonResult(new 
            { 
                success = true, 
                message = "¡Matrícula completada exitosamente!",
                redirectUrl = "/Account/Login"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar callback de Culqi");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    public class CulqiTokenRequest
    {
        public string Token { get; set; } = string.Empty;
        public string? Email { get; set; }
    }

    public class PendingRegistrationData
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
