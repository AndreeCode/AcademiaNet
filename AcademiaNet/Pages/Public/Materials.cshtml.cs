using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;

namespace Academic.Pages.Public;

public class MaterialsModel : PageModel
{
    private readonly AcademicContext _context;
    public MaterialsModel(AcademicContext context) => _context = context;

    public List<Material> Materials { get; set; } = new();

    public async Task OnGetAsync(int? week)
    {
        if (week.HasValue)
            Materials = await _context.Materiales.Where(m => m.Week == week.Value).Include(m => m.Curso).Include(m => m.Ciclo).ToListAsync();
        else
            Materials = await _context.Materiales.Include(m => m.Curso).Include(m => m.Ciclo).OrderBy(m => m.Week).ToListAsync();
    }
}
