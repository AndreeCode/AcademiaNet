using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Academic.Data;
using Academic.Models;
using System.ComponentModel.DataAnnotations;

namespace Academic.Pages.Admin;

[Authorize(Roles = "Admin,Coordinador")]
public class EditCycleModel : PageModel
{
    private readonly AcademicContext _context;

    public EditCycleModel(AcademicContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public Ciclo? Ciclo { get; set; }

    public class InputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del ciclo es requerido")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [Required]
        public DateTime MatriculaInicio { get; set; }

        [Required]
        public DateTime MatriculaFin { get; set; }

        [Required]
        [Range(0, 10000)]
        public int Vacantes { get; set; }

        [Required]
        public ModalidadCiclo Modalidad { get; set; }

        [Required(ErrorMessage = "El monto de matrícula es requerido")]
        [Range(1.00, 10000.00, ErrorMessage = "El monto debe estar entre S/ 1.00 y S/ 10,000.00")]
        [Display(Name = "Monto de Matrícula")]
        public decimal MontoMatricula { get; set; } = 1.00m;
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            // Si no se proporciona ID, editar el ciclo más reciente
            Ciclo = await _context.Ciclos.OrderByDescending(c => c.Id).FirstOrDefaultAsync();
        }
        else
        {
            Ciclo = await _context.Ciclos.FindAsync(id.Value);
        }

        if (Ciclo == null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = Ciclo.Id,
            Nombre = Ciclo.Nombre,
            FechaInicio = Ciclo.FechaInicio,
            FechaFin = Ciclo.FechaFin,
            MatriculaInicio = Ciclo.MatriculaInicio ?? DateTime.Now,
            MatriculaFin = Ciclo.MatriculaFin ?? DateTime.Now.AddDays(30),
            Vacantes = Ciclo.Vacantes,
            Modalidad = Ciclo.Modalidad,
            MontoMatricula = Ciclo.MontoMatricula
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Ciclo = await _context.Ciclos.FindAsync(Input.Id);
            return Page();
        }

        var ciclo = await _context.Ciclos.FindAsync(Input.Id);
        if (ciclo == null)
        {
            return NotFound();
        }

        // Validaciones
        if (Input.FechaFin <= Input.FechaInicio)
        {
            ModelState.AddModelError("Input.FechaFin", "La fecha de fin debe ser posterior a la fecha de inicio.");
            Ciclo = ciclo;
            return Page();
        }

        if (Input.MatriculaFin <= Input.MatriculaInicio)
        {
            ModelState.AddModelError("Input.MatriculaFin", "La fecha de fin de matrícula debe ser posterior a la fecha de inicio.");
            Ciclo = ciclo;
            return Page();
        }

        if (Input.MatriculaInicio > Input.FechaInicio)
        {
            ModelState.AddModelError("Input.MatriculaInicio", "La matrícula debe iniciar antes del inicio del ciclo.");
            Ciclo = ciclo;
            return Page();
        }

        try
        {
            ciclo.Nombre = Input.Nombre;
            ciclo.FechaInicio = Input.FechaInicio;
            ciclo.FechaFin = Input.FechaFin;
            ciclo.MatriculaInicio = Input.MatriculaInicio;
            ciclo.MatriculaFin = Input.MatriculaFin;
            ciclo.Vacantes = Input.Vacantes;
            ciclo.Modalidad = Input.Modalidad;
            ciclo.MontoMatricula = Input.MontoMatricula;

            await _context.SaveChangesAsync();

            StatusMessage = $"Ciclo '{ciclo.Nombre}' actualizado exitosamente.";
            return RedirectToPage("/Admin/Dashboard");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al actualizar el ciclo: {ex.Message}";
            Ciclo = ciclo;
            return Page();
        }
    }
}
