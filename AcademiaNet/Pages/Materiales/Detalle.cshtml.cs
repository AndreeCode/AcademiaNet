using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;

namespace Academic.Pages.Materiales;

[Authorize(Roles = "Admin,Tutor,Profesor")]
public class DetalleModel : PageModel
{
    private readonly AcademicContext _context;

    public DetalleModel(AcademicContext context)
    {
        _context = context;
    }

    public Material Material { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        Material = await _context.Materiales
            .Include(m => m.Ciclo)
            .Include(m => m.Semana)
            .Include(m => m.Salon)
            .ThenInclude(s => s!.Sede)
            .Include(m => m.Tutor)
            .Include(m => m.Curso)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (Material == null)
        {
            return NotFound();
        }

        return Page();
    }
}
