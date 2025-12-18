using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;
using Academic.Services;

namespace Academic.Pages.Materiales;

[Authorize(Roles = "Admin,Tutor,Coordinador")]
public class IndexModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly FileStorageService _fileStorage;

    public IndexModel(AcademicContext context, FileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public List<Material> Materiales { get; set; } = new();
    public string? SearchQuery { get; set; }
    public int? FilterCicloId { get; set; }
    public List<Ciclo> Ciclos { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync(string? search, int? cicloId)
    {
        SearchQuery = search;
        FilterCicloId = cicloId;

        Ciclos = await _context.Ciclos.OrderByDescending(c => c.FechaInicio).ToListAsync();

        var query = _context.Materiales
            .Include(m => m.Ciclo)
            .Include(m => m.Semana)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(m => m.Title.Contains(search) || m.Description.Contains(search));
        }

        if (cicloId.HasValue)
        {
            query = query.Where(m => m.CicloId == cicloId.Value);
        }

        Materiales = await query
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var material = await _context.Materiales.FindAsync(id);
        if (material == null)
        {
            StatusMessage = "Error: Material no encontrado.";
            return RedirectToPage();
        }

        try
        {
            // Eliminar archivo físico si existe
            if (!string.IsNullOrEmpty(material.FileUrl) && material.TipoMaterial != TipoMaterial.Enlace)
            {
                var fullPath = _fileStorage.GetFullPath(material.FileUrl);
                _fileStorage.DeleteFile(fullPath);
            }

            _context.Materiales.Remove(material);
            await _context.SaveChangesAsync();

            StatusMessage = "Material eliminado exitosamente.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al eliminar el material: {ex.Message}";
        }

        return RedirectToPage();
    }

    public string GetFileSizeString(long? bytes)
    {
        if (!bytes.HasValue || bytes == 0) return "N/A";

        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes.Value;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
