using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Academic.Data;
using Microsoft.EntityFrameworkCore;
using Academic.Models;

namespace Academic.Pages.Admin;

[Authorize(Roles = "Admin")]
public class DashboardModel : PageModel
{
    private readonly AcademicContext _context;
    public DashboardModel(AcademicContext context) => _context = context;

    public List<Academic.Models.Tutor> Tutores { get; set; } = new();
    public List<Academic.Models.Ciclo> Ciclos { get; set; } = new();
    public int VacantesDisponibles { get; set; }
    public Ciclo? CicloActual { get; set; }
    public int TotalAlumnos { get; set; }
    public int TotalMatriculas { get; set; }
    public int MatriculasPendientes { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        Tutores = await _context.Tutores.OrderBy(t => t.Nombre).ToListAsync();
        Ciclos = await _context.Ciclos.OrderByDescending(c => c.FechaInicio).ToListAsync();
        
        CicloActual = Ciclos.FirstOrDefault();
        
        if (CicloActual != null)
        {
            if (CicloActual.Vacantes > 0)
            {
                var countPagadas = await _context.Matriculas
                    .CountAsync(m => m.CicloId == CicloActual.Id && m.EstadoPago == Academic.Models.EstadoPago.Pagado);
                VacantesDisponibles = Math.Max(0, CicloActual.Vacantes - countPagadas);
            }
            else
            {
                VacantesDisponibles = 0; // Ilimitadas
            }

            TotalMatriculas = await _context.Matriculas
                .CountAsync(m => m.CicloId == CicloActual.Id);
            
            MatriculasPendientes = await _context.Matriculas
                .CountAsync(m => m.CicloId == CicloActual.Id && m.EstadoPago == Academic.Models.EstadoPago.Pendiente);
        }

        TotalAlumnos = await _context.Alumnos.CountAsync();
    }
}
