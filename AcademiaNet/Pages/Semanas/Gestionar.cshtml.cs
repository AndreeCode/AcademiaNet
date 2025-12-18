using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;
using System.ComponentModel.DataAnnotations;

namespace Academic.Pages.Semanas;

[Authorize(Roles = "Admin,Coordinador,Profesor,Tutor")]
public class GestionarModel : PageModel
{
    private readonly AcademicContext _context;

    public GestionarModel(AcademicContext context)
    {
        _context = context;
    }

    public List<Semana> Semanas { get; set; } = new();
    public List<SelectListItem> CiclosList { get; set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public int? SelectedCicloId { get; set; }

    public class InputModel
    {
        [Required]
        public int CicloId { get; set; }

        [Required]
        [Range(1, 52, ErrorMessage = "El número de semana debe estar entre 1 y 52")]
        public int NumeroSemana { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [StringLength(200)]
        public string? Tema { get; set; }

        [StringLength(1000)]
        public string? Descripcion { get; set; }
    }

    public async Task OnGetAsync(int? cicloId)
    {
        SelectedCicloId = cicloId;
        await LoadData(cicloId);
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadData(Input.CicloId);
            return Page();
        }

        try
        {
            // Verificar que no exista ya una semana con ese número en el ciclo
            var exists = await _context.Semanas
                .AnyAsync(s => s.CicloId == Input.CicloId && s.NumeroSemana == Input.NumeroSemana);

            if (exists)
            {
                StatusMessage = $"Error: Ya existe la Semana {Input.NumeroSemana} en este ciclo.";
                await LoadData(Input.CicloId);
                return Page();
            }

            var semana = new Semana
            {
                CicloId = Input.CicloId,
                NumeroSemana = Input.NumeroSemana,
                FechaInicio = Input.FechaInicio,
                FechaFin = Input.FechaFin,
                Tema = Input.Tema,
                Descripcion = Input.Descripcion,
                IsActive = true
            };

            _context.Semanas.Add(semana);
            await _context.SaveChangesAsync();

            StatusMessage = "Semana creada exitosamente.";
            return RedirectToPage(new { cicloId = Input.CicloId });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al crear la semana: {ex.Message}";
            await LoadData(Input.CicloId);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var semana = await _context.Semanas
            .Include(s => s.Materiales)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (semana == null)
        {
            StatusMessage = "Error: Semana no encontrada.";
            return RedirectToPage();
        }

        try
        {
            if (semana.Materiales.Any())
            {
                StatusMessage = "Error: No se puede eliminar una semana que tiene materiales asociados.";
                return RedirectToPage(new { cicloId = semana.CicloId });
            }

            _context.Semanas.Remove(semana);
            await _context.SaveChangesAsync();

            StatusMessage = "Semana eliminada exitosamente.";
            return RedirectToPage(new { cicloId = semana.CicloId });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al eliminar la semana: {ex.Message}";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostGenerateWeeksAsync(int cicloId, int numWeeks)
    {
        try
        {
            var ciclo = await _context.Ciclos.FindAsync(cicloId);
            if (ciclo == null)
            {
                StatusMessage = "Error: Ciclo no encontrado.";
                return RedirectToPage();
            }

            var existingWeeks = await _context.Semanas
                .Where(s => s.CicloId == cicloId)
                .Select(s => s.NumeroSemana)
                .ToListAsync();

            var startDate = ciclo.FechaInicio;
            int created = 0;

            for (int i = 1; i <= numWeeks; i++)
            {
                if (existingWeeks.Contains(i)) continue;

                var semana = new Semana
                {
                    CicloId = cicloId,
                    NumeroSemana = i,
                    FechaInicio = startDate.AddDays((i - 1) * 7),
                    FechaFin = startDate.AddDays(i * 7 - 1),
                    Tema = $"Semana {i}",
                    IsActive = true
                };

                _context.Semanas.Add(semana);
                created++;
            }

            await _context.SaveChangesAsync();
            StatusMessage = $"Se generaron {created} semanas automáticamente.";
            return RedirectToPage(new { cicloId });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al generar semanas: {ex.Message}";
            return RedirectToPage();
        }
    }

    private async Task LoadData(int? cicloId)
    {
        var ciclos = await _context.Ciclos
            .OrderByDescending(c => c.FechaInicio)
            .ToListAsync();

        CiclosList = ciclos.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Nombre,
            Selected = c.Id == cicloId
        }).ToList();

        if (cicloId.HasValue)
        {
            Semanas = await _context.Semanas
                .Include(s => s.Materiales)
                .Where(s => s.CicloId == cicloId.Value)
                .OrderBy(s => s.NumeroSemana)
                .ToListAsync();

            // Prellenar datos para nueva semana
            var ciclo = ciclos.FirstOrDefault(c => c.Id == cicloId.Value);
            if (ciclo != null)
            {
                Input.CicloId = ciclo.Id;
                var lastWeek = Semanas.LastOrDefault();
                Input.NumeroSemana = (lastWeek?.NumeroSemana ?? 0) + 1;
                Input.FechaInicio = lastWeek?.FechaFin.AddDays(1) ?? ciclo.FechaInicio;
                Input.FechaFin = Input.FechaInicio.AddDays(6);
            }
        }
    }
}
