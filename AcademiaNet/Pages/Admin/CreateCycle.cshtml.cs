using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Academic.Data;
using Academic.Models;
using System.ComponentModel.DataAnnotations;

namespace Academic.Pages.Admin;

[Authorize(Roles = "Admin,Coordinador")]
public class CreateCycleModel : PageModel
{
    private readonly AcademicContext _context;

    public CreateCycleModel(AcademicContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "El nombre del ciclo es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre del Ciclo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        [Display(Name = "Fecha de Inicio del Ciclo")]
        public DateTime FechaInicio { get; set; } = DateTime.Now.AddDays(30);

        [Required(ErrorMessage = "La fecha de fin es requerida")]
        [Display(Name = "Fecha de Fin del Ciclo")]
        public DateTime FechaFin { get; set; } = DateTime.Now.AddDays(150);

        [Required(ErrorMessage = "La fecha de inicio de matrícula es requerida")]
        [Display(Name = "Inicio de Matrícula")]
        public DateTime MatriculaInicio { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "La fecha de fin de matrícula es requerida")]
        [Display(Name = "Fin de Matrícula")]
        public DateTime MatriculaFin { get; set; } = DateTime.Now.AddDays(25);

        [Required(ErrorMessage = "El número de vacantes es requerido")]
        [Range(0, 10000, ErrorMessage = "Las vacantes deben estar entre 0 y 10000")]
        [Display(Name = "Número de Vacantes")]
        public int Vacantes { get; set; } = 100;

        [Required(ErrorMessage = "La modalidad es requerida")]
        [Display(Name = "Modalidad")]
        public ModalidadCiclo Modalidad { get; set; } = ModalidadCiclo.Hibrido;

        [Required(ErrorMessage = "El monto de matrícula es requerido")]
        [Range(1.00, 10000.00, ErrorMessage = "El monto debe estar entre S/ 1.00 y S/ 10,000.00")]
        [Display(Name = "Monto de Matrícula")]
        public decimal MontoMatricula { get; set; } = 1.00m;
    }

    public void OnGet()
    {
        // Configurar fechas por defecto
        Input = new InputModel
        {
            FechaInicio = DateTime.Now.AddDays(30),
            FechaFin = DateTime.Now.AddDays(150),
            MatriculaInicio = DateTime.Now,
            MatriculaFin = DateTime.Now.AddDays(25),
            Vacantes = 100,
            Modalidad = ModalidadCiclo.Hibrido,
            MontoMatricula = 1.00m
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Validaciones de fechas
        if (Input.FechaFin <= Input.FechaInicio)
        {
            ModelState.AddModelError("Input.FechaFin", "La fecha de fin debe ser posterior a la fecha de inicio.");
            return Page();
        }

        if (Input.MatriculaFin <= Input.MatriculaInicio)
        {
            ModelState.AddModelError("Input.MatriculaFin", "La fecha de fin de matrícula debe ser posterior a la fecha de inicio de matrícula.");
            return Page();
        }

        if (Input.MatriculaInicio > Input.FechaInicio)
        {
            ModelState.AddModelError("Input.MatriculaInicio", "La matrícula debe iniciar antes del inicio del ciclo.");
            return Page();
        }

        try
        {
            var ciclo = new Ciclo
            {
                Nombre = Input.Nombre,
                FechaInicio = Input.FechaInicio,
                FechaFin = Input.FechaFin,
                MatriculaInicio = Input.MatriculaInicio,
                MatriculaFin = Input.MatriculaFin,
                Vacantes = Input.Vacantes,
                Modalidad = Input.Modalidad,
                MontoMatricula = Input.MontoMatricula
            };

            _context.Ciclos.Add(ciclo);
            await _context.SaveChangesAsync();

            // Crear semanas automáticamente
            var duracionCiclo = (Input.FechaFin - Input.FechaInicio).Days;
            var numeroSemanas = Math.Min(duracionCiclo / 7, 20); // Máximo 20 semanas

            for (int i = 1; i <= numeroSemanas; i++)
            {
                var semana = new Semana
                {
                    NumeroSemana = i,
                    CicloId = ciclo.Id,
                    FechaInicio = Input.FechaInicio.AddDays((i - 1) * 7),
                    FechaFin = Input.FechaInicio.AddDays(i * 7 - 1),
                    Tema = $"Semana {i}",
                    Descripcion = $"Contenido de la semana {i}",
                    IsActive = true
                };
                _context.Semanas.Add(semana);
            }

            await _context.SaveChangesAsync();

            StatusMessage = $"Ciclo '{ciclo.Nombre}' creado exitosamente con {numeroSemanas} semanas.";
            return RedirectToPage("/Admin/Dashboard");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al crear el ciclo: {ex.Message}";
            return Page();
        }
    }
}
