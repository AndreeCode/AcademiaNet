using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Academic.Pages.Public;

public class MaterialsModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public MaterialsModel(AcademicContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public List<Material> Materials { get; set; } = new();
    public string? NotAuthorizedMessage { get; set; }

    public async Task OnGetAsync(int? week, string? q)
    {
        // Require authenticated student who is active and matriculated in the current ciclo
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            NotAuthorizedMessage = "Debes iniciar sesión para acceder a la biblioteca.";
            return;
        }

        var alumno = await _context.Alumnos
            .Include(a => a.Matriculas)
            .Include(a => a.Salon)
                .ThenInclude(s => s.Profesor)
            .FirstOrDefaultAsync(a => a.Email == user.Email);

        if (alumno == null)
        {
            NotAuthorizedMessage = "No se encontró un perfil de alumno vinculado a este usuario.";
            return;
        }

        if (!alumno.IsActive)
        {
            NotAuthorizedMessage = "Tu cuenta está desactivada. Contacta a tu tutor o administración.";
            return;
        }

        var currentCiclo = await _context.Ciclos.OrderByDescending(c => c.Id).FirstOrDefaultAsync();
        if (currentCiclo == null)
        {
            NotAuthorizedMessage = "No hay ciclos disponibles.";
            return;
        }

        var hasMatricula = alumno.Matriculas.Any(m => m.CicloId == currentCiclo.Id && m.EstadoPago != "Cancelado");
        if (!hasMatricula)
        {
            NotAuthorizedMessage = "No estás matriculado en el ciclo activo; la biblioteca está disponible solo para alumnos matriculados.";
            return;
        }

        // Build base query: materials for student's salon or for the course taught by the salon's professor
        var salonId = alumno.SalonId;
        var profesorId = alumno.Salon?.ProfesorId;

        IQueryable<Material> query = _context.Materiales
            .Include(m => m.Curso)
            .Include(m => m.Ciclo)
            .Include(m => m.Tutor)
            .AsQueryable();

        if (salonId.HasValue)
        {
            query = query.Where(m => m.SalonId == salonId.Value || (m.Curso != null && m.Curso.ProfesorId == profesorId));
        }
        else
        {
            // if no salon assigned, show nothing
            NotAuthorizedMessage = "No tienes aula asignada. Contacta a tu tutor para más información.";
            return;
        }

        if (week.HasValue)
            query = query.Where(m => m.Week == week.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(m => m.Title.Contains(term) || m.Description.Contains(term) || (m.Curso != null && m.Curso.Nombre.Contains(term)) || (m.Tutor != null && (m.Tutor.Nombre + " " + m.Tutor.Apellido).Contains(term)) );
        }

        Materials = await query.OrderBy(m => m.Week).ToListAsync();
    }
}
