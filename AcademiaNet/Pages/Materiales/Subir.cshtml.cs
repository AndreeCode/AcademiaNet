using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;
using Academic.Services;
using System.ComponentModel.DataAnnotations;

namespace Academic.Pages.Materiales;

[Authorize(Roles = "Admin,Tutor,Coordinador,Profesor")]
public class SubirModel : PageModel
{
    private readonly AcademicContext _context;
    private readonly FileStorageService _fileStorage;

    public SubirModel(AcademicContext context, FileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public List<SelectListItem> CiclosList { get; set; } = new();
    public List<SelectListItem> SemanasList { get; set; } = new();
    public List<SelectListItem> TipoMaterialList { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un ciclo")]
        public int CicloId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una semana")]
        public int SemanaId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el tipo de material")]
        public TipoMaterial TipoMaterial { get; set; }

        public IFormFile? File { get; set; }

        [Url]
        [StringLength(500)]
        [Display(Name = "URL (si es un enlace)")]
        public string? Url { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadSelectLists();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectLists();
            return Page();
        }

        try
        {
            var semana = await _context.Semanas
                .Include(s => s.Ciclo)
                .FirstOrDefaultAsync(s => s.Id == Input.SemanaId);

            if (semana == null)
            {
                StatusMessage = "Error: La semana seleccionada no existe.";
                await LoadSelectLists();
                return Page();
            }

            var material = new Material
            {
                Title = Input.Title,
                Description = Input.Description ?? "",
                CicloId = Input.CicloId,
                SemanaId = Input.SemanaId,
                Week = semana.NumeroSemana,
                TipoMaterial = Input.TipoMaterial,
                CreatedAt = DateTime.UtcNow
            };

            // Si es un enlace, guardar la URL directamente
            if (Input.TipoMaterial == TipoMaterial.Enlace && !string.IsNullOrEmpty(Input.Url))
            {
                material.FileUrl = Input.Url;
                material.FileName = "Enlace externo";
            }
            // Si se subió un archivo
            else if (Input.File != null && Input.File.Length > 0)
            {
                var (success, filePath, error) = await _fileStorage.SaveFileAsync(
                    Input.File,
                    semana.Ciclo!.Nombre,
                    semana.NumeroSemana,
                    Input.TipoMaterial
                );

                if (!success)
                {
                    StatusMessage = $"Error: {error}";
                    await LoadSelectLists();
                    return Page();
                }

                material.FileUrl = _fileStorage.GetRelativePath(filePath!);
                material.FileName = Input.File.FileName;
                material.FileSize = Input.File.Length;
            }
            else
            {
                StatusMessage = "Error: Debe proporcionar un archivo o una URL.";
                await LoadSelectLists();
                return Page();
            }

            _context.Materiales.Add(material);
            await _context.SaveChangesAsync();

            StatusMessage = "Material subido exitosamente.";
            return RedirectToPage("/Materiales/Index");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al subir el material: {ex.Message}";
            await LoadSelectLists();
            return Page();
        }
    }

    private async Task LoadSelectLists()
    {
        var ciclos = await _context.Ciclos
            .OrderByDescending(c => c.FechaInicio)
            .ToListAsync();

        CiclosList = ciclos.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Nombre
        }).ToList();

        var semanas = await _context.Semanas
            .Include(s => s.Ciclo)
            .OrderBy(s => s.CicloId)
            .ThenBy(s => s.NumeroSemana)
            .ToListAsync();

        SemanasList = semanas.Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = $"{s.Ciclo?.Nombre} - Semana {s.NumeroSemana}: {s.Tema}"
        }).ToList();

        TipoMaterialList = Enum.GetValues<TipoMaterial>()
            .Select(t => new SelectListItem
            {
                Value = ((int)t).ToString(),
                Text = t.ToString()
            }).ToList();
    }
}
