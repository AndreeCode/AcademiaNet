using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Academic.Models;

namespace Academic.Data;

public sealed class AcademicContext : IdentityDbContext
{
    public AcademicContext(DbContextOptions<AcademicContext> options) : base(options) { }

    public DbSet<Sede> Sedes => Set<Sede>();
    public DbSet<Salon> Salones => Set<Salon>();
    public DbSet<Tutor> Tutores => Set<Tutor>();
    public DbSet<Profesor> Profesores => Set<Profesor>();
    public DbSet<Alumno> Alumnos => Set<Alumno>();
    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<Ciclo> Ciclos => Set<Ciclo>();
    public DbSet<Semana> Semanas => Set<Semana>();
    public DbSet<Matricula> Matriculas => Set<Matricula>();
    public DbSet<Horario> Horarios => Set<Horario>();
    public DbSet<TutorSalon> TutorSalones => Set<TutorSalon>();
    public DbSet<Material> Materiales => Set<Material>();
    public DbSet<ConfiguracionPasarela> ConfiguracionPasarelas => Set<ConfiguracionPasarela>();
    public DbSet<Nota> Notas => Set<Nota>();
    public DbSet<Apoderado> Apoderados => Set<Apoderado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure TutorSalon as join table with composite key
        modelBuilder.Entity<TutorSalon>(entity =>
        {
            entity.HasKey(ts => new { ts.TutorId, ts.SalonId });

            entity.HasOne(ts => ts.Tutor)
                  .WithMany(t => t.TutorSalones)
                  .HasForeignKey(ts => ts.TutorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ts => ts.Salon)
                  .WithMany(s => s.TutorSalones)
                  .HasForeignKey(ts => ts.SalonId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // decimal precision for Matricula.Monto and PaidAmount
        modelBuilder.Entity<Matricula>().Property(m => m.Monto).HasPrecision(18, 2);
        modelBuilder.Entity<Matricula>().Property(m => m.PaidAmount).HasPrecision(18, 2);

        // Additional model configuration can go here
    }
}
