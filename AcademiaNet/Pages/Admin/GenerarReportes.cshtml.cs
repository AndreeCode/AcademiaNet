using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;
using Academic.Services;

namespace Academic.Pages.Admin;

[Authorize(Roles = "Admin,Coordinador")]
public class GenerarReportesModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly PdfService _pdfService;
    private readonly ILogger<GenerarReportesModel> _logger;

    public GenerarReportesModel(
        AcademicContext context,
        PdfService pdfService,
        ILogger<GenerarReportesModel> logger)
    {
        _context = context;
        _pdfService = pdfService;
        _logger = logger;
    }

    [BindProperty]
    public int CicloId { get; set; }

    [BindProperty]
    public string TipoReporte { get; set; } = "Matriculados";

    public SelectList CiclosSelectList { get; set; } = new SelectList(Enumerable.Empty<Ciclo>());
    public List<Ciclo> Ciclos { get; set; } = new();

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostGenerarPdfAsync()
    {
        try
        {
            byte[] pdfBytes;

            switch (TipoReporte)
            {
                case "Matriculados":
                    pdfBytes = await _pdfService.GenerarReporteMatriculadosAsync(CicloId);
                    break;

                case "Estadisticas":
                    pdfBytes = await _pdfService.GenerarReporteEstadisticasAsync(CicloId);
                    break;

                default:
                    ErrorMessage = "Tipo de reporte no válido";
                    await LoadDataAsync();
                    return Page();
            }

            var ciclo = await _context.Ciclos.FindAsync(CicloId);
            var fileName = $"{TipoReporte}_{ciclo?.Nombre.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte PDF");
            ErrorMessage = "Error al generar el reporte: " + ex.Message;
            await LoadDataAsync();
            return Page();
        }
    }

    private async Task LoadDataAsync()
    {
        Ciclos = await _context.Ciclos
            .Include(c => c.Matriculas.Where(m => m.EstadoPago == EstadoPago.Pagado))
            .OrderByDescending(c => c.Id)
            .ToListAsync();

        CiclosSelectList = new SelectList(
            Ciclos.Select(c => new
            {
                c.Id,
                Display = $"{c.Nombre} ({c.FechaInicio:dd/MM/yyyy} - {c.FechaFin:dd/MM/yyyy})"
            }),
            "Id",
            "Display"
        );

        if (!Ciclos.Any())
        {
            ErrorMessage = "No hay ciclos disponibles para generar reportes";
        }
    }
}
