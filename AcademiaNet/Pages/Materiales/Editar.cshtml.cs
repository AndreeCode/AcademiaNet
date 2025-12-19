using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;
using System.ComponentModel.DataAnnotations;

namespace Academic.Pages.Materiales;

[Authorize(Roles = "Admin,Tutor,Profesor")]
public class EditarModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly ILogger<EditarModel> _logger;

    public EditarModel(AcademicContext context, ILogger<EditarModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    [BindProperty]
    public MaterialInputModel Input { get; set; } = new();

    public class MaterialInputModel
    {
        public int Id { get; set; }
        [Required] [StringLength(200)] public string Title { get; set; } = string.Empty;
        [StringLength(1000)] public string? Description { get; set; }
        [Required] public int CicloId { get; set; }
        public int? SemanaId { get; set; }
        public int? SalonId { get; set; }
        public TipoMaterial TipoMaterial { get; set; }
        public string? FileUrl { get; set; }
    }

    public SelectList Ciclos { get; set; } = default!;
    public SelectList Semanas { get; set; } = default!;
    public SelectList Salones { get; set; } = default!;

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var material = await _context.Materiales
            .Include(m => m.Ciclo)
            .Include(m => m.Semana)
            .Include(m => m.Salon)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (material == null)
        {
            return NotFound();
        }

        Input = new MaterialInputModel
        {
            Id = material.Id,
            Title = material.Title,
            Description = material.Description,
            CicloId = material.CicloId ?? 0,
            SemanaId = material.SemanaId,
            SalonId = material.SalonId,
            TipoMaterial = material.TipoMaterial,
            FileUrl = material.FileUrl
        };

        await LoadSelectListsAsync(Input.CicloId);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync(Input.CicloId);
            return Page();
        }

        var material = await _context.Materiales.FindAsync(Input.Id);
        if (material == null)
        {
            return NotFound();
        }

        try
        {
            material.Title = Input.Title;
            material.Description = Input.Description;
            material.CicloId = Input.CicloId;
            material.SemanaId = Input.SemanaId;
            material.SalonId = Input.SalonId;
            material.TipoMaterial = Input.TipoMaterial;

            if (Input.TipoMaterial == TipoMaterial.Enlace && !string.IsNullOrEmpty(Input.FileUrl))
            {
                material.FileUrl = Input.FileUrl;
            }

            await _context.SaveChangesAsync();

            StatusMessage = "Material actualizado exitosamente.";
            return RedirectToPage("/Materiales/Detalle", new { id = material.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar material");
            StatusMessage = "Error: " + ex.Message;
            await LoadSelectListsAsync(Input.CicloId);
            return Page();
        }
    }

    private async Task LoadSelectListsAsync(int selectedCicloId = 0)
    {
        var ciclos = await _context.Ciclos.OrderByDescending(c => c.Id).ToListAsync();
        Ciclos = new SelectList(ciclos, "Id", "Nombre", selectedCicloId);

        if (selectedCicloId > 0)
        {
            var semanas = await _context.Semanas
                .Where(s => s.CicloId == selectedCicloId)
                .OrderBy(s => s.NumeroSemana)
                .ToListAsync();
            Semanas = new SelectList(
                semanas.Select(s => new { s.Id, Nombre = $"Semana {s.NumeroSemana} - {s.Tema}" }),
                "Id",
                "Nombre"
            );
        }
        else
        {
            Semanas = new SelectList(Enumerable.Empty<Semana>());
        }

        var salones = await _context.Salones
            .Include(s => s.Sede)
            .OrderBy(s => s.Nombre)
            .ToListAsync();
        Salones = new SelectList(
            salones.Select(s => new { s.Id, Nombre = $"{s.Nombre} - {s.Sede?.Nombre}" }),
            "Id",
            "Nombre"
        );
    }
}
