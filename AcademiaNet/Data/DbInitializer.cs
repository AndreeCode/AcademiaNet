using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Academic.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Linq;
using System;

namespace Academic.Data;

public static class DbInitializer
{
    public static async Task ApplyMigrationsAsync(AcademicContext context, ILogger? logger = null)
    {
        if (!await context.Database.CanConnectAsync())
            throw new InvalidOperationException("Unable to connect to the database.");

        var migrations = context.Database.GetMigrations();
        if (migrations != null && migrations.Any())
        {
            await context.Database.MigrateAsync();
            return;
        }

        logger?.LogWarning("No EF Core migrations were found in the assembly. Falling back to EnsureCreated(). Consider creating migrations with 'dotnet ef migrations add'.");
        await context.Database.EnsureCreatedAsync();

        // Try to add missing columns and tables that may exist in the model but not in the existing DB (dev only)
        try
        {
            await EnsureCicloColumnsAsync(context, logger);
            await EnsureMaterialesTableAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to ensure schema fixes");
        }
    }

    private static async Task EnsureCicloColumnsAsync(AcademicContext context, ILogger? logger)
    {
        // Only for SQL Server: check for column existence and add if missing
        var addCommands = new List<string>();

        var hasMatriculaInicio = await ColumnExistsAsync(context, "Ciclos", "MatriculaInicio");
        if (!hasMatriculaInicio)
        {
            addCommands.Add("ALTER TABLE [Ciclos] ADD [MatriculaInicio] datetime2 NULL;");
        }

        var hasMatriculaFin = await ColumnExistsAsync(context, "Ciclos", "MatriculaFin");
        if (!hasMatriculaFin)
        {
            addCommands.Add("ALTER TABLE [Ciclos] ADD [MatriculaFin] datetime2 NULL;");
        }

        var hasVacantes = await ColumnExistsAsync(context, "Ciclos", "Vacantes");
        if (!hasVacantes)
        {
            // add non-nullable with default 0
            addCommands.Add("ALTER TABLE [Ciclos] ADD [Vacantes] int NOT NULL CONSTRAINT DF_Ciclos_Vacantes DEFAULT(0);");
        }

        if (addCommands.Count == 0) return;

        foreach (var cmd in addCommands)
        {
            logger?.LogInformation("Executing schema fix: {Cmd}", cmd);
            await context.Database.ExecuteSqlRawAsync(cmd);
        }
    }

    private static async Task EnsureMaterialesTableAsync(AcademicContext context, ILogger? logger)
    {
        var tableExists = await TableExistsAsync(context, "Materiales");
        if (tableExists) return;

        logger?.LogInformation("Materiales table missing — creating table.");

        // Create table matching the Material model
        var createSql = @"CREATE TABLE [Materiales](
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CicloId] INT NULL,
    [CreatedAt] DATETIME2 NOT NULL,
    [CursoId] INT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [FileUrl] NVARCHAR(MAX) NULL,
    [Title] NVARCHAR(MAX) NULL,
    [Week] INT NOT NULL
);
";
        await context.Database.ExecuteSqlRawAsync(createSql);

        // Optionally create FK constraints if referenced tables exist
        try
        {
            var cicloExists = await TableExistsAsync(context, "Ciclos");
            var cursosExists = await TableExistsAsync(context, "Cursos");
            if (cicloExists)
            {
                var fk1 = "ALTER TABLE [Materiales] ADD CONSTRAINT FK_Materiales_Ciclos FOREIGN KEY (CicloId) REFERENCES [Ciclos](Id) ON DELETE SET NULL;";
                await context.Database.ExecuteSqlRawAsync(fk1);
            }
            if (cursosExists)
            {
                var fk2 = "ALTER TABLE [Materiales] ADD CONSTRAINT FK_Materiales_Cursos FOREIGN KEY (CursoId) REFERENCES [Cursos](Id) ON DELETE SET NULL;";
                await context.Database.ExecuteSqlRawAsync(fk2);
            }
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Could not add foreign keys for Materiales; continuing without FKs.");
        }
    }

    private static async Task<bool> TableExistsAsync(AcademicContext context, string tableName)
    {
        var sql = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @table";
        var conn = context.Database.GetDbConnection();
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = sql;
            var p1 = cmd.CreateParameter(); p1.ParameterName = "@table"; p1.Value = tableName; cmd.Parameters.Add(p1);

            if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            await conn.CloseAsync();
            if (result is int i) return i > 0;
            if (result is long l) return l > 0;
            return Convert.ToInt32(result) > 0;
        }
    }

    private static async Task<bool> ColumnExistsAsync(AcademicContext context, string tableName, string columnName)
    {
        var sql = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @table AND COLUMN_NAME = @column";
        var conn = context.Database.GetDbConnection();
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = sql;
            var p1 = cmd.CreateParameter(); p1.ParameterName = "@table"; p1.Value = tableName; cmd.Parameters.Add(p1);
            var p2 = cmd.CreateParameter(); p2.ParameterName = "@column"; p2.Value = columnName; cmd.Parameters.Add(p2);

            if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            await conn.CloseAsync();
            if (result is int i) return i > 0;
            if (result is long l) return l > 0;
            return Convert.ToInt32(result) > 0;
        }
    }

    public static async Task SeedAsync(AcademicContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger? logger = null)
    {
        // Ensure identity schema available
        async Task<bool> TryEnsureIdentityTablesAsync()
        {
            try
            {
                await roleManager.Roles.FirstOrDefaultAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "RoleManager probe failed, attempting EnsureCreated().");
                try
                {
                    await context.Database.EnsureCreatedAsync();
                    // Retry
                    await roleManager.Roles.FirstOrDefaultAsync();
                    return true;
                }
                catch (Exception ex2)
                {
                    logger?.LogError(ex2, "RoleManager probe failed after EnsureCreated().");
                    return false;
                }
            }
        }

        if (!await TryEnsureIdentityTablesAsync())
            throw new InvalidOperationException("Identity tables (e.g. AspNetRoles) do not exist and could not be created. Ensure migrations are present or create the schema manually.");

        // Roles and role claims
        var roles = new[] { "Admin", "Profesor", "Tutor", "Alumno", "Coordinador" };
        var roleClaims = new Dictionary<string, Claim[]>
        {
            ["Admin"] = new[] { new Claim("permission", "all") },
            ["Profesor"] = new[] { new Claim("permission", "manage_courses") },
            ["Tutor"] = new[] { new Claim("permission", "manage_tutors") },
            ["Coordinador"] = new[] { new Claim("permission", "manage_enrollment") },
            ["Alumno"] = new[] { new Claim("permission", "student_access") }
        };

        foreach (var roleName in roles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                role = new IdentityRole(roleName);
                var res = await roleManager.CreateAsync(role);
                if (!res.Succeeded)
                    logger?.LogError("Failed creating role {Role}: {Errors}", roleName, string.Join(';', res.Errors.Select(e => e.Description)));
            }

            // ensure claims for role
            if (role != null && roleClaims.TryGetValue(roleName, out var claims))
            {
                var existing = await roleManager.GetClaimsAsync(role);
                foreach (var c in claims)
                {
                    if (!existing.Any(ec => ec.Type == c.Type && ec.Value == c.Value))
                    {
                        await roleManager.AddClaimAsync(role, c);
                    }
                }
            }
        }

        // Predefined users with roles
        var predefined = new[] {
            (Email: "admin@academia.local", Role: "Admin", Password: "Admin123!"),
            (Email: "coordinador@academia.local", Role: "Coordinador", Password: "Coord123!"),
            (Email: "profesor@academia.local", Role: "Profesor", Password: "Prof123!"),
            (Email: "tutor@academia.local", Role: "Tutor", Password: "Tutor123!"),
            (Email: "alumno@academia.local", Role: "Alumno", Password: "Alumno123!")
        };

        foreach (var u in predefined)
        {
            var user = await userManager.FindByEmailAsync(u.Email);
            if (user is null)
            {
                user = new IdentityUser { UserName = u.Email, Email = u.Email, EmailConfirmed = true };
                var create = await userManager.CreateAsync(user, u.Password);
                if (!create.Succeeded)
                {
                    logger?.LogError("Failed to create user {Email}: {Errors}", u.Email, string.Join(';', create.Errors.Select(e => e.Description)));
                    continue;
                }
            }

            if (!await userManager.IsInRoleAsync(user, u.Role))
            {
                await userManager.AddToRoleAsync(user, u.Role);
            }
        }

        // Seed domain data (idempotent) - create a ciclo 2025-II and multiple salons, tutors, profs, students, materials
        if (!await context.Ciclos.AnyAsync(c => c.Nombre == "Ciclo 2025-II"))
        {
            var ciclo = new Ciclo { Nombre = "Ciclo 2025-II", FechaInicio = new DateTime(2025, 8, 1), FechaFin = new DateTime(2025, 12, 31), Vacantes = 0 };
            context.Ciclos.Add(ciclo);

            var sede = new Sede { Nombre = "Zoe - Ayacucho", Direccion = "Calle Ficticia 123" };
            context.Sedes.Add(sede);

            // professors and tutors
            var profesor = new Profesor { Nombre = "Juan", Apellido = "Perez", Email = "profesor@academia.local" };
            var profesor2 = new Profesor { Nombre = "Luis", Apellido = "Gomez", Email = "luis@academia.local" };
            context.Profesores.AddRange(profesor, profesor2);

            var tutor = new Tutor { Nombre = "Ana", Apellido = "Lopez", Email = "tutor@academia.local" };
            var tutor2 = new Tutor { Nombre = "Marcos", Apellido = "Rojas", Email = "marcos@academia.local" };
            context.Tutores.AddRange(tutor, tutor2);

            // courses
            var curso = new Curso { Nombre = "Matemática Básica", Profesor = profesor };
            var curso2 = new Curso { Nombre = "Física I", Profesor = profesor2 };
            context.Cursos.AddRange(curso, curso2);

            // salons
            var salonA = new Salon { Nombre = "A1", Sede = sede, Profesor = profesor };
            var salonB = new Salon { Nombre = "B1", Sede = sede, Profesor = profesor2 };
            context.Salones.AddRange(salonA, salonB);

            // assign tutors to salons
            context.TutorSalones.AddRange(new TutorSalon { Tutor = tutor, Salon = salonA }, new TutorSalon { Tutor = tutor2, Salon = salonB });

            // students
            var students = new[] {
                new Alumno { Nombre = "Carlos", Apellido = "Sanchez", Email = "alumno@academia.local", Salon = salonA },
                new Alumno { Nombre = "María", Apellido = "Diaz", Email = "maria@academia.local", Salon = salonA },
                new Alumno { Nombre = "Pedro", Apellido = "Lopez", Email = "pedro@academia.local", Salon = salonB }
            };
            context.Alumnos.AddRange(students);

            // matriculas
            foreach (var s in students)
            {
                context.Matriculas.Add(new Matricula { Alumno = s, Ciclo = ciclo, Monto = 100m, Moneda = "PEN", EstadoPago = "Pagado", CreatedAt = DateTime.UtcNow });
            }

            // horarios
            context.Horarios.AddRange(
                new Horario { Salon = salonA, Dia = DayOfWeek.Monday, HoraInicio = TimeSpan.FromHours(9), HoraFin = TimeSpan.FromHours(11) },
                new Horario { Salon = salonB, Dia = DayOfWeek.Tuesday, HoraInicio = TimeSpan.FromHours(15), HoraFin = TimeSpan.FromHours(17) }
            );

            // materials per week
            for (int week = 1; week <= 12; week++)
            {
                context.Materiales.Add(new Material { Title = $"Semana {week} - Matemáticas", Description = "Apuntes y ejercicios.", Week = week, Curso = curso, Ciclo = ciclo, FileUrl = null });
                context.Materiales.Add(new Material { Title = $"Semana {week} - Física", Description = "Apuntes y laboratorio.", Week = week, Curso = curso2, Ciclo = ciclo, FileUrl = null });
            }

            await context.SaveChangesAsync();
        }
    }
}
