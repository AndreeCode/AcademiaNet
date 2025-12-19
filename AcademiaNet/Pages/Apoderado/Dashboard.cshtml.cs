using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;
using AlumnoModel = Academic.Models.Alumno;
using System.Security.Claims;

namespace Academic.Pages.Apoderado;

[Authorize(Roles = "Apoderado")]
public class DashboardModel : PageModel
{
    private readonly AcademicContext _context;

    public DashboardModel(AcademicContext context)
    {
        _context = context;
    }

    public Models.Apoderado? ApoderadoActual { get; set; }
    public AlumnoModel? Hijo { get; set; }
    public List<Nota> NotasHijo { get; set; } = new();
    public List<Matricula> MatriculasHijo { get; set; } = new();
    public decimal PromedioGeneral { get; set; }

    public async Task OnGetAsync()
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        
        ApoderadoActual = await _context.Apoderados
            .Include(a => a.Alumno)
            .ThenInclude(al => al!.Salon)
            .ThenInclude(s => s!.Sede)
            .FirstOrDefaultAsync(a => a.Email == userEmail);

        if (ApoderadoActual?.Alumno != null)
        {
            Hijo = ApoderadoActual.Alumno;

            NotasHijo = await _context.Notas
                .Include(n => n.Ciclo)
                .Include(n => n.Salon)
                .Where(n => n.AlumnoId == Hijo.Id && n.IsActive)
                .OrderByDescending(n => n.FechaEvaluacion)
                .ToListAsync();

            MatriculasHijo = await _context.Matriculas
                .Include(m => m.Ciclo)
                .Where(m => m.AlumnoId == Hijo.Id)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            PromedioGeneral = Hijo.PromedioGeneral ?? 0;
        }
    }
}
