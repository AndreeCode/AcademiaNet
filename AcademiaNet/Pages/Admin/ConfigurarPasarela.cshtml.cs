using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;

namespace Academic.Pages.Admin;

[Authorize(Roles = "Admin")]
public class ConfigurarPasarelaModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly ILogger<ConfigurarPasarelaModel> _logger;

    public ConfigurarPasarelaModel(AcademicContext context, ILogger<ConfigurarPasarelaModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    [BindProperty]
    public TipoPasarela PasarelaSeleccionada { get; set; }

    public ConfiguracionPasarela? ConfiguracionActual { get; set; }

    public string? MensajeExito { get; set; }
    public string? MensajeError { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        ConfiguracionActual = await _context.ConfiguracionPasarelas.FirstOrDefaultAsync();
        
        if (ConfiguracionActual == null)
        {
            // Crear configuración por defecto si no existe
            ConfiguracionActual = new ConfiguracionPasarela
            {
                PasarelaActiva = TipoPasarela.SinPasarela,
                UltimaModificacion = DateTime.UtcNow
            };
            _context.ConfiguracionPasarelas.Add(ConfiguracionActual);
            await _context.SaveChangesAsync();
        }

        PasarelaSeleccionada = ConfiguracionActual.PasarelaActiva;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var config = await _context.ConfiguracionPasarelas.FirstOrDefaultAsync();
            
            if (config == null)
            {
                config = new ConfiguracionPasarela();
                _context.ConfiguracionPasarelas.Add(config);
            }

            config.PasarelaActiva = PasarelaSeleccionada;
            config.UltimaModificacion = DateTime.UtcNow;
            config.ModificadoPor = User.Identity?.Name;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Pasarela de pago cambiada a: {Pasarela} por {Usuario}", 
                PasarelaSeleccionada, User.Identity?.Name);

            MensajeExito = $"Pasarela de pago actualizada exitosamente a: {ObtenerNombrePasarela(PasarelaSeleccionada)}";
            ConfiguracionActual = config;

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar configuración de pasarela");
            MensajeError = "Ocurrió un error al actualizar la configuración.";
            return Page();
        }
    }

    private string ObtenerNombrePasarela(TipoPasarela tipo)
    {
        return tipo switch
        {
            TipoPasarela.SinPasarela => "Sin Pasarela (Matrícula Manual)",
            TipoPasarela.MercadoPago => "MercadoPago",
            TipoPasarela.Culqi => "Culqi",
            _ => "Desconocida"
        };
    }
}
