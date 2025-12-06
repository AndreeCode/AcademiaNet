using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Academic.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Linq;
using System;
using System.Globalization;

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
            await EnsureTutorColumnsAsync(context, logger);
            await EnsureAlumnoColumnsAsync(context, logger);
            await EnsureMaterialesTableAsync(context, logger);
            await EnsureMaterialesColumnsAsync(context, logger);
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

    private static async Task EnsureAlumnoColumnsAsync(AcademicContext context, ILogger? logger)
    {
        var addCommands = new List<string>();
        var hasIsActive = await ColumnExistsAsync(context, "Alumnos", "IsActive");
        if (!hasIsActive)
        {
            // add IsActive as bit with default 1
            addCommands.Add("ALTER TABLE [Alumnos] ADD [IsActive] bit NOT NULL CONSTRAINT DF_Alumnos_IsActive DEFAULT(1);");
        }

        if (addCommands.Count == 0) return;

        foreach (var cmd in addCommands)
        {
            logger?.LogInformation("Executing schema fix: {Cmd}", cmd);
            await context.Database.ExecuteSqlRawAsync(cmd);
        }
    }

    private static async Task EnsureTutorColumnsAsync(AcademicContext context, ILogger? logger)
    {
        var addCommands = new List<string>();

        var hasIsActive = await ColumnExistsAsync(context, "Tutores", "IsActive");
        if (!hasIsActive)
        {
            // add IsActive as bit with default 1
            addCommands.Add("ALTER TABLE [Tutores] ADD [IsActive] bit NOT NULL CONSTRAINT DF_Tutores_IsActive DEFAULT(1);");
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

        // Create table matching the Material model (base columns)
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

    private static async Task EnsureMaterialesColumnsAsync(AcademicContext context, ILogger? logger)
    {
        var addCommands = new List<string>();

        var hasSalonId = await ColumnExistsAsync(context, "Materiales", "SalonId");
        if (!hasSalonId)
            addCommands.Add("ALTER TABLE [Materiales] ADD [SalonId] int NULL;");

        var hasTutorId = await ColumnExistsAsync(context, "Materiales", "TutorId");
        if (!hasTutorId)
            addCommands.Add("ALTER TABLE [Materiales] ADD [TutorId] int NULL;");

        var hasCreatedById = await ColumnExistsAsync(context, "Materiales", "CreatedById");
        if (!hasCreatedById)
            addCommands.Add("ALTER TABLE [Materiales] ADD [CreatedById] int NULL;");

        if (addCommands.Count == 0) return;

        foreach (var cmd in addCommands)
        {
            logger?.LogInformation("Executing schema fix: {Cmd}", cmd);
            await context.Database.ExecuteSqlRawAsync(cmd);
        }

        // add foreign keys if referenced tables exist
        try
        {
            if (await TableExistsAsync(context, "Salones"))
            {
                var fkSalon = "IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Materiales_Salones') ALTER TABLE [Materiales] ADD CONSTRAINT FK_Materiales_Salones FOREIGN KEY (SalonId) REFERENCES [Salones](Id) ON DELETE SET NULL;";
                await context.Database.ExecuteSqlRawAsync(fkSalon);
            }
            if (await TableExistsAsync(context, "Tutores"))
            {
                var fkTutor = "IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Materiales_Tutores') ALTER TABLE [Materiales] ADD CONSTRAINT FK_Materiales_Tutores FOREIGN KEY (TutorId) REFERENCES [Tutores](Id) ON DELETE SET NULL;";
                await context.Database.ExecuteSqlRawAsync(fkTutor);
            }
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Could not add foreign keys for new Materiales columns; continuing.");
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

        // Ensure sample domain identity users exist even if domain data already seeded
        await EnsureSampleDomainIdentityUsersAsync(userManager, logger);

        // Ensure Tutor domain entities and at least one Salon and a welcome Material exist for each Identity tutor
        await EnsureTutorsDomainAsync(context, userManager, logger);

        // Seed domain data (idempotent) - create ciclo and complete content: sedes, salones, tutores, profesores, coordinador, alumnos, horarios, materials
        if (!await context.Ciclos.AnyAsync(c => c.Nombre == "Ciclo 2025-II"))
        {
            var ciclo = new Ciclo
            {
                Nombre = "Ciclo 2025-II",
                FechaInicio = new DateTime(2025, 8, 1),
                FechaFin = new DateTime(2025, 12, 31),
                Vacantes = 100
            };
            context.Ciclos.Add(ciclo);

            var sede = new Sede { Nombre = "Zoe - Ayacucho", Direccion = "Calle Ficticia 123" };
            var sede2 = new Sede { Nombre = "Zoe - Huamanga", Direccion = "Av. Principal 456" };
            context.Sedes.AddRange(sede, sede2);

            // Professors
            var prof1 = new Profesor { Nombre = "Juan", Apellido = "Perez", Email = "profesor@academia.local" };
            var prof2 = new Profesor { Nombre = "Luis", Apellido = "Gomez", Email = "luis@academia.local" };
            context.Profesores.AddRange(prof1, prof2);

            // Tutors (one active, one inactive)
            var tutor1 = new Tutor { Nombre = "Ana", Apellido = "Lopez", Email = "tutor@academia.local", IsActive = true };
            var tutor2 = new Tutor { Nombre = "Marcos", Apellido = "Rojas", Email = "marcos@academia.local", IsActive = false };
            context.Tutores.AddRange(tutor1, tutor2);

            // Ensure identity users for coordinator, tutors and professors exist and have roles
            var coordUser = await userManager.FindByEmailAsync("coordinador@academia.local");
            if (coordUser == null)
            {
                coordUser = new IdentityUser { Email = "coordinador@academia.local", UserName = "coordinador@academia.local", EmailConfirmed = true };
                await userManager.CreateAsync(coordUser, "Coord123!");
                await userManager.AddToRoleAsync(coordUser, "Coordinador");
            }

            var p1User = await userManager.FindByEmailAsync(prof1.Email);
            if (p1User == null)
            {
                p1User = new IdentityUser { Email = prof1.Email, UserName = prof1.Email, EmailConfirmed = true };
                await userManager.CreateAsync(p1User, "Prof123!");
                await userManager.AddToRoleAsync(p1User, "Profesor");
            }

            var p2User = await userManager.FindByEmailAsync(prof2.Email);
            if (p2User == null)
            {
                p2User = new IdentityUser { Email = prof2.Email, UserName = prof2.Email, EmailConfirmed = true };
                await userManager.CreateAsync(p2User, "Prof123!");
                await userManager.AddToRoleAsync(p2User, "Profesor");
            }

            var t1User = await userManager.FindByEmailAsync(tutor1.Email);
            if (t1User == null)
            {
                t1User = new IdentityUser { Email = tutor1.Email, UserName = tutor1.Email, EmailConfirmed = true };
                await userManager.CreateAsync(t1User, "Tutor123!");
                await userManager.AddToRoleAsync(t1User, "Tutor");
            }

            var t2User = await userManager.FindByEmailAsync(tutor2.Email);
            if (t2User == null)
            {
                t2User = new IdentityUser { Email = tutor2.Email, UserName = tutor2.Email, EmailConfirmed = true };
                await userManager.CreateAsync(t2User, "Tutor123!");
                await userManager.AddToRoleAsync(t2User, "Tutor");
            }

            // Salons: ensure each tutor has at least 2 salons
            var salon1 = new Salon { Nombre = "A1", Sede = sede, Profesor = prof1 };
            var salon2 = new Salon { Nombre = "A2", Sede = sede, Profesor = prof1 };
            var salon3 = new Salon { Nombre = "B1", Sede = sede2, Profesor = prof2 };
            var salon4 = new Salon { Nombre = "B2", Sede = sede2, Profesor = prof2 };
            context.Salones.AddRange(salon1, salon2, salon3, salon4);

            // Assign tutors to salons
            context.TutorSalones.AddRange(
                new TutorSalon { Tutor = tutor1, Salon = salon1 },
                new TutorSalon { Tutor = tutor1, Salon = salon2 },
                new TutorSalon { Tutor = tutor2, Salon = salon3 },
                new TutorSalon { Tutor = tutor2, Salon = salon4 }
            );

            // Students: at least 2 per salon, with one inactive per some salons
            var alumnos = new List<Alumno>
            {
                new Alumno { Nombre = "Carlos", Apellido = "Sanchez", Email = "carlos@academia.local", Salon = salon1, IsActive = true },
                new Alumno { Nombre = "María", Apellido = "Diaz", Email = "maria@academia.local", Salon = salon1, IsActive = false },

                new Alumno { Nombre = "Pedro", Apellido = "Lopez", Email = "pedro@academia.local", Salon = salon2, IsActive = true },
                new Alumno { Nombre = "Lucia", Apellido = "Vargas", Email = "lucia@academia.local", Salon = salon2, IsActive = true },

                new Alumno { Nombre = "José", Apellido = "Martinez", Email = "jose@academia.local", Salon = salon3, IsActive = true },
                new Alumno { Nombre = "AnaL", Apellido = "Torres", Email = "analt@academia.local", Salon = salon3, IsActive = false },

                new Alumno { Nombre = "Miguel", Apellido = "Quispe", Email = "miguel@academia.local", Salon = salon4, IsActive = true },
                new Alumno { Nombre = "Sofia", Apellido = "Reyna", Email = "sofia@academia.local", Salon = salon4, IsActive = true }
            };
            context.Alumnos.AddRange(alumnos);

            // Create identity users for alumnos and assign role
            foreach (var a in alumnos)
            {
                var u = await userManager.FindByEmailAsync(a.Email);
                if (u == null)
                {
                    u = new IdentityUser { Email = a.Email, UserName = a.Email, EmailConfirmed = true };
                    await userManager.CreateAsync(u, "Alumno123!");
                    await userManager.AddToRoleAsync(u, "Alumno");
                }
            }

            // Matriculas: only active students
            foreach (var a in alumnos.Where(x => x.IsActive))
            {
                context.Matriculas.Add(new Matricula { Alumno = a, Ciclo = ciclo, Monto = 100m, Moneda = "PEN", EstadoPago = "Pagado", CreatedAt = DateTime.UtcNow });
            }

            // Horarios per salon
            context.Horarios.AddRange(
                new Horario { Salon = salon1, Dia = DayOfWeek.Monday, HoraInicio = TimeSpan.FromHours(8), HoraFin = TimeSpan.FromHours(10) },
                new Horario { Salon = salon1, Dia = DayOfWeek.Wednesday, HoraInicio = TimeSpan.FromHours(8), HoraFin = TimeSpan.FromHours(10) },
                new Horario { Salon = salon2, Dia = DayOfWeek.Tuesday, HoraInicio = TimeSpan.FromHours(10), HoraFin = TimeSpan.FromHours(12) },
                new Horario { Salon = salon3, Dia = DayOfWeek.Monday, HoraInicio = TimeSpan.FromHours(15), HoraFin = TimeSpan.FromHours(17) },
                new Horario { Salon = salon4, Dia = DayOfWeek.Thursday, HoraInicio = TimeSpan.FromHours(16), HoraFin = TimeSpan.FromHours(18) }
            );

            // Materials: each salon gets materials per week, tied to tutor and salon and course
            for (int w = 1; w <= 12; w++)
            {
                context.Materiales.Add(new Material { Title = $"Semana {w} - Matemáticas ({salon1.Nombre})", Description = "Apuntes y ejercicios.", Week = w, Curso = prof1.Cursos.FirstOrDefault() ?? null, Ciclo = ciclo, FileUrl = null, Salon = salon1, Tutor = tutor1, CreatedAt = DateTime.UtcNow });
                context.Materiales.Add(new Material { Title = $"Semana {w} - Matemáticas ({salon2.Nombre})", Description = "Apuntes y ejercicios adicionales.", Week = w, Curso = prof1.Cursos.FirstOrDefault() ?? null, Ciclo = ciclo, FileUrl = null, Salon = salon2, Tutor = tutor1, CreatedAt = DateTime.UtcNow });
                context.Materiales.Add(new Material { Title = $"Semana {w} - Física ({salon3.Nombre})", Description = "Apuntes y laboratorio.", Week = w, Curso = prof2.Cursos.FirstOrDefault() ?? null, Ciclo = ciclo, FileUrl = null, Salon = salon3, Tutor = tutor2, CreatedAt = DateTime.UtcNow });
                context.Materiales.Add(new Material { Title = $"Semana {w} - Física ({salon4.Nombre})", Description = "Ejercicios y guías.", Week = w, Curso = prof2.Cursos.FirstOrDefault() ?? null, Ciclo = ciclo, FileUrl = null, Salon = salon4, Tutor = tutor2, CreatedAt = DateTime.UtcNow });
            }

            await context.SaveChangesAsync();
        }
    }

    private static async Task EnsureSampleDomainIdentityUsersAsync(UserManager<IdentityUser> userManager, ILogger? logger)
    {
        // Sample domain users corresponding to seeded domain data
        var samples = new (string Email, string Role, string Password)[]
        {
            ("carlos@academia.local", "Alumno", "Alumno123!"),
            ("maria@academia.local", "Alumno", "Alumno123!"),
            ("pedro@academia.local", "Alumno", "Alumno123!"),
            ("lucia@academia.local", "Alumno", "Alumno123!"),
            ("jose@academia.local", "Alumno", "Alumno123!"),
            ("analt@academia.local", "Alumno", "Alumno123!"),
            ("miguel@academia.local", "Alumno", "Alumno123!"),
            ("sofia@academia.local", "Alumno", "Alumno123!"),
            ("profesor@academia.local", "Profesor", "Prof123!"),
            ("luis@academia.local", "Profesor", "Prof123!"),
            ("tutor@academia.local", "Tutor", "Tutor123!"),
            ("marcos@academia.local", "Tutor", "Tutor123!")
        };

        foreach (var s in samples)
        {
            try
            {
                var u = await userManager.FindByEmailAsync(s.Email);
                if (u == null)
                {
                    u = new IdentityUser { Email = s.Email, UserName = s.Email, EmailConfirmed = true };
                    var res = await userManager.CreateAsync(u, s.Password);
                    if (!res.Succeeded)
                    {
                        logger?.LogWarning("Could not create sample user {Email}: {Errors}", s.Email, string.Join(';', res.Errors.Select(e => e.Description)));
                        continue;
                    }
                    logger?.LogInformation("Created sample user {Email} with password {Pwd}", s.Email, s.Password);
                }

                if (!await userManager.IsInRoleAsync(u, s.Role))
                {
                    await userManager.AddToRoleAsync(u, s.Role);
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Failed ensuring sample user {Email}", s.Email);
            }
        }
    }

    private static async Task EnsureTutorsDomainAsync(AcademicContext context, UserManager<IdentityUser> userManager, ILogger? logger)
    {
        try
        {
            var tutorUsers = await userManager.GetUsersInRoleAsync("Tutor");
            if (tutorUsers == null || tutorUsers.Count == 0) return;

            // Ensure at least one Sede exists
            var defaultSede = await context.Sedes.FirstOrDefaultAsync();
            if (defaultSede == null)
            {
                defaultSede = new Sede { Nombre = "Sede Principal", Direccion = "Dirección principal" };
                context.Sedes.Add(defaultSede);
                await context.SaveChangesAsync();
            }

            foreach (var iu in tutorUsers)
            {
                if (string.IsNullOrWhiteSpace(iu.Email)) continue;

                var existingTutor = await context.Tutores.FirstOrDefaultAsync(t => t.Email == iu.Email);
                if (existingTutor == null)
                {
                    var localName = iu.Email.Split('@')[0];
                    var parts = localName.Split('.', StringSplitOptions.RemoveEmptyEntries);
                    var nombre = parts.Length > 0 ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(parts[0]) : localName;
                    var apellido = parts.Length > 1 ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(parts[1]) : "";

                    existingTutor = new Tutor { Nombre = nombre, Apellido = apellido, Email = iu.Email, IsActive = true };
                    context.Tutores.Add(existingTutor);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Created Tutor domain entry for {Email}", iu.Email);
                }

                // Ensure tutor has at least one salon assigned
                var hasSalon = await context.TutorSalones.AnyAsync(ts => ts.TutorId == existingTutor.Id);
                if (!hasSalon)
                {
                    var salon = new Salon { Nombre = existingTutor.Nombre + " - Aula 1", Sede = defaultSede };
                    context.Salones.Add(salon);
                    await context.SaveChangesAsync();

                    context.TutorSalones.Add(new TutorSalon { TutorId = existingTutor.Id, SalonId = salon.Id });
                    await context.SaveChangesAsync();

                    logger?.LogInformation("Assigned salon {Salon} to tutor {Tutor}", salon.Nombre, existingTutor.Email);

                    // Add a welcome material for the new salon
                    var ciclo = await context.Ciclos.OrderByDescending(c => c.Id).FirstOrDefaultAsync();
                    var material = new Material
                    {
                        Title = "Bienvenida al curso",
                        Description = "Material de bienvenida.",
                        Week = 1,
                        SalonId = salon.Id,
                        TutorId = existingTutor.Id,
                        Ciclo = ciclo,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Materiales.Add(material);
                    await context.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Failed to ensure tutors domain data");
        }
    }

}