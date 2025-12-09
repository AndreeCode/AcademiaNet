using Microsoft.AspNetCore.Authorization;
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

    public async Task OnGetAsync()
    {
        Tutores = await _context.Tutores.ToListAsync();
        Ciclos = await _context.Ciclos.OrderByDescending(c => c.Id).ToListAsync();

        var current = Ciclos.FirstOrDefault();
        if (current != null && current.Vacantes > 0)
        {
            // count only matriculas pagadas to compute used vacancies
            var count = await _context.Matriculas.CountAsync(m => m.CicloId == current.Id && m.EstadoPago == Academic.Models.EstadoPago.Pagado);
            VacantesDisponibles = Math.Max(0, current.Vacantes - count);
        }
    }
}
