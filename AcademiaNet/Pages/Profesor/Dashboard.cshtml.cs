using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Academic.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Academic.Pages.Profesor;

[Authorize(Roles = "Profesor")]
public class DashboardModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public DashboardModel(AcademicContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public Academic.Models.Profesor? ProfesorModel { get; set; }
    public List<Academic.Models.Curso> Cursos { get; set; } = new();
    public List<Academic.Models.Horario> Horarios { get; set; } = new();

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return;

        ProfesorModel = await _context.Profesores.FirstOrDefaultAsync(p => p.Email == user.Email);
        if (ProfesorModel == null) return;

        Cursos = await _context.Cursos.Where(c => c.ProfesorId == ProfesorModel.Id).ToListAsync();

        Horarios = await _context.Horarios
            .Where(h => h.Salon != null && h.Salon.ProfesorId == ProfesorModel.Id)
            .Include(h => h.Salon)
            .ToListAsync();
    }
}
