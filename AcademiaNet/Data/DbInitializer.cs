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
            await EnsureMatriculaColumnsAsync(context, logger);
            await EnsureTutorColumnsAsync(context, logger);
            await EnsureAlumnoColumnsAsync(context, logger);
            await EnsureAlumnoProfileColumnsAsync(context, logger);
            await EnsureSemanaTableAsync(context, logger);
            await EnsureMaterialesTableAsync(context, logger);
            await EnsureMaterialesColumnsAsync(context, logger);
            await EnsureConfiguracionPasarelaTableAsync(context, logger);
            await EnsureNotasTableAsync(context, logger);
            await EnsureApoderadosTableAsync(context, logger);
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

        var hasMontoMatricula = await ColumnExistsAsync(context, "Ciclos", "MontoMatricula");
        if (!hasMontoMatricula)
        {
            // add non-nullable with default 1.00
            addCommands.Add("ALTER TABLE [Ciclos] ADD [MontoMatricula] decimal(18,2) NOT NULL CONSTRAINT DF_Ciclos_MontoMatricula DEFAULT(1.00);");
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

    private static async Task EnsureAlumnoProfileColumnsAsync(AcademicContext context, ILogger? logger)
    {
        var addCommands = new List<string>();

        var hasTelefono = await ColumnExistsAsync(context, "Alumnos", "Telefono");
        if (!hasTelefono)
            addCommands.Add("ALTER TABLE [Alumnos] ADD [Telefono] nvarchar(20) NULL;");

        var hasDireccion = await ColumnExistsAsync(context, "Alumnos", "Direccion");
        if (!hasDireccion)
            addCommands.Add("ALTER TABLE [Alumnos] ADD [Direccion] nvarchar(200) NULL;");

        var hasDNI = await ColumnExistsAsync(context, "Alumnos", "DNI");
        if (!hasDNI)
            addCommands.Add("ALTER TABLE [Alumnos] ADD [DNI] nvarchar(20) NULL;");

        var hasNombreApoderado = await ColumnExistsAsync(context, "Alumnos", "NombreApoderado");
        if (!hasNombreApoderado)
            addCommands.Add("ALTER TABLE [Alumnos] ADD [NombreApoderado] nvarchar(150) NULL;");

        var hasTelefonoApoderado = await ColumnExistsAsync(context, "Alumnos", "TelefonoApoderado");
        if (!hasTelefonoApoderado)
            addCommands.Add("ALTER TABLE [Alumnos] ADD [TelefonoApoderado] nvarchar(20) NULL;");

        if (addCommands.Count == 0) return;

        foreach (var cmd in addCommands)
        {
            logger?.LogInformation("Executing schema fix: {Cmd}", cmd);
            await context.Database.ExecuteSqlRawAsync(cmd);
        }
    }

    private static async Task EnsureSemanaTableAsync(AcademicContext context, ILogger? logger)
    {
        var tableExists = await TableExistsAsync(context, "Semanas");
        if (tableExists) return;

        logger?.LogInformation("Semanas table missing — creating table.");

        var createSql = @"CREATE TABLE [Semanas](
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [NumeroSemana] INT NOT NULL,
    [CicloId] INT NOT NULL,
    [FechaInicio] DATETIME2 NOT NULL,
    [FechaFin] DATETIME2 NOT NULL,
    [Tema] NVARCHAR(200) NULL,
    [Descripcion] NVARCHAR(MAX) NULL,
    [IsActive] BIT NOT NULL DEFAULT(1),
    CONSTRAINT FK_Semanas_Ciclos FOREIGN KEY (CicloId) REFERENCES [Ciclos](Id) ON DELETE CASCADE
);";
        await context.Database.ExecuteSqlRawAsync(createSql);
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

        var hasSemanaId = await ColumnExistsAsync(context, "Materiales", "SemanaId");
        if (!hasSemanaId)
            addCommands.Add("ALTER TABLE [Materiales] ADD [SemanaId] int NULL;");

        var hasFileName = await ColumnExistsAsync(context, "Materiales", "FileName");
        if (!hasFileName)
            addCommands.Add("ALTER TABLE [Materiales] ADD [FileName] nvarchar(500) NULL;");

        var hasFileSize = await ColumnExistsAsync(context, "Materiales", "FileSize");
        if (!hasFileSize)
            addCommands.Add("ALTER TABLE [Materiales] ADD [FileSize] bigint NULL;");

        var hasTipoMaterial = await ColumnExistsAsync(context, "Materiales", "TipoMaterial");
        if (!hasTipoMaterial)
            addCommands.Add("ALTER TABLE [Materiales] ADD [TipoMaterial] int NOT NULL DEFAULT(2);");

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
            if (await TableExistsAsync(context, "Semanas"))
            {
                var fkSemana = "IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Materiales_Semanas') ALTER TABLE [Materiales] ADD CONSTRAINT FK_Materiales_Semanas FOREIGN KEY (SemanaId) REFERENCES [Semanas](Id) ON DELETE SET NULL;";
                await context.Database.ExecuteSqlRawAsync(fkSemana);
            }
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Could not add foreign keys for new Materiales columns; continuing.");
        }
    }

    private static async Task EnsureMatriculaColumnsAsync(AcademicContext context, ILogger? logger)
    {
        var addCommands = new List<string>();

        var hasMercadoPagoInitPoint = await ColumnExistsAsync(context, "Matriculas", "MercadoPagoInitPoint");
        if (!hasMercadoPagoInitPoint)
        {
            addCommands.Add("ALTER TABLE [Matriculas] ADD [MercadoPagoInitPoint] nvarchar(500) NULL;");
        }

        var hasMercadoPagoPreferenceId = await ColumnExistsAsync(context, "Matriculas", "MercadoPagoPreferenceId");
        if (!hasMercadoPagoPreferenceId)
        {
            addCommands.Add("ALTER TABLE [Matriculas] ADD [MercadoPagoPreferenceId] nvarchar(200) NULL;");
        }

        var hasMercadoPagoPaymentId = await ColumnExistsAsync(context, "Matriculas", "MercadoPagoPaymentId");
        if (!hasMercadoPagoPaymentId)
        {
            addCommands.Add("ALTER TABLE [Matriculas] ADD [MercadoPagoPaymentId] nvarchar(200) NULL;");
        }

        var hasFechaPago = await ColumnExistsAsync(context, "Matriculas", "FechaPago");
        if (!hasFechaPago)
        {
            addCommands.Add("ALTER TABLE [Matriculas] ADD [FechaPago] datetime2 NULL;");
        }

        var hasPaidAmount = await ColumnExistsAsync(context, "Matriculas", "PaidAmount");
        if (!hasPaidAmount)
        {
            addCommands.Add("ALTER TABLE [Matriculas] ADD [PaidAmount] decimal(18,2) NULL;");
        }

        // Nuevas columnas para Culqi
        var hasTipoPasarela = await ColumnExistsAsync(context, "Matriculas", "TipoPasarela");
        if (!hasTipoPasarela)
        {
            addCommands.Add("ALTER TABLE [Matriculas] ADD [TipoPasarela] int NOT NULL CONSTRAINT DF_Matriculas_TipoPasarela DEFAULT(0);");
        }

        var hasCulqiChargeId = await ColumnExistsAsync(context, "Matriculas", "CulqiChargeId");
        if (!hasCulqiChargeId)
        {
            addCommands.Add("ALTER TABLE [Matriculas] ADD [CulqiChargeId] nvarchar(200) NULL;");
        }

        var hasCulqiTokenId = await ColumnExistsAsync(context, "Matriculas", "CulqiTokenId");
        if (!hasCulqiTokenId)
        {
            addCommands.Add("ALTER TABLE [Matriculas] ADD [CulqiTokenId] nvarchar(200) NULL;");
        }

        var hasCulqiOrderId = await ColumnExistsAsync(context, "Matriculas", "CulqiOrderId");
        if (!hasCulqiOrderId)
        {
            addCommands.Add("ALTER TABLE [Matriculas] ADD [CulqiOrderId] nvarchar(200) NULL;");
        }

        var hasObservaciones = await ColumnExistsAsync(context, "Matriculas", "Observaciones");
        if (!hasObservaciones)
        {
            addCommands.Add("ALTER TABLE [Matriculas] ADD [Observaciones] nvarchar(MAX) NULL;");
        }

        if (addCommands.Count == 0) return;

        foreach (var cmd in addCommands)
        {
            logger?.LogInformation("Executing schema fix: {Cmd}", cmd);
            await context.Database.ExecuteSqlRawAsync(cmd);
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
        var roles = new[] { "Admin", "Profesor", "Tutor", "Alumno", "Coordinador", "Apoderado" };
        var roleClaims = new Dictionary<string, Claim[]>
        {
            ["Admin"] = new[] { new Claim("permission", "all") },
            ["Profesor"] = new[] { new Claim("permission", "manage_courses") },
            ["Tutor"] = new[] { new Claim("permission", "manage_tutors") },
            ["Coordinador"] = new[] { new Claim("permission", "manage_enrollment") },
            ["Alumno"] = new[] { new Claim("permission", "student_access") },
            ["Apoderado"] = new[] { new Claim("permission", "parent_access") }
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
                FechaInicio = DateTime.UtcNow.AddDays(30),  // Inicia en 30 días
                FechaFin = DateTime.UtcNow.AddDays(150),     // Termina en 150 días (aprox 5 meses)
                Vacantes = 100,
                MatriculaInicio = DateTime.UtcNow.AddDays(-5),  // Matrícula abierta desde hace 5 días
                MatriculaFin = DateTime.UtcNow.AddDays(25),     // Matrícula cierra en 25 días
                Modalidad = ModalidadCiclo.Hibrido,
                MontoMatricula = 1.00m  // Monto por defecto
            };
            context.Ciclos.Add(ciclo);
            await context.SaveChangesAsync();

            // Crear semanas para el ciclo (12 semanas)
            for (int i = 1; i <= 12; i++)
            {
                var semana = new Semana
                {
                    NumeroSemana = i,
                    CicloId = ciclo.Id,
                    FechaInicio = ciclo.FechaInicio.AddDays((i - 1) * 7),
                    FechaFin = ciclo.FechaInicio.AddDays(i * 7 - 1),
                    Tema = $"Semana {i}",
                    Descripcion = $"Contenido de la semana {i}",
                    IsActive = true
                };
                context.Semanas.Add(semana);
            }
            await context.SaveChangesAsync();

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
            var salon3 = new Salon { Nombre = "B1", Sede = sede2,Profesor=prof2 };
            var salon4 = new Salon { Nombre = "B2", Sede = sede2,Profesor=prof2 };
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
                context.Matriculas.Add(new Matricula { Alumno = a, Ciclo = ciclo, Monto = 100m, Moneda = "PEN", EstadoPago = EstadoPago.Pagado, CreatedAt = DateTime.UtcNow });
            }

            // Horarios per salon
            context.Horarios.AddRange(
                new Horario { Salon = salon1, Dia = DayOfWeek.Monday, HoraInicio = TimeSpan.FromHours(8), HoraFin = TimeSpan.FromHours(10) },
                new Horario { Salon = salon1, Dia = DayOfWeek.Wednesday, HoraInicio = TimeSpan.FromHours(8), HoraFin = TimeSpan.FromHours(10) },
                new Horario { Salon = salon2, Dia = DayOfWeek.Tuesday, HoraInicio = TimeSpan.FromHours(10), HoraFin = TimeSpan.FromHours(12) },
                new Horario { Salon = salon3, Dia = DayOfWeek.Monday, HoraInicio = TimeSpan.FromHours(15), HoraFin = TimeSpan.FromHours(17) },
                new Horario { Salon = salon4, Dia = DayOfWeek.Thursday, HoraInicio = TimeSpan.FromHours(16), HoraFin = TimeSpan.FromHours(18) }
            );

            // Materials: create materials for ALL salons and ALL weeks
            var semanas = await context.Semanas.Where(s => s.CicloId == ciclo.Id).ToListAsync();
            
            // MATERIALES PARA TODAS LAS SEMANAS Y TODOS LOS SALONES
            foreach (var semana in semanas)
            {
                int weekNum = semana.NumeroSemana;

                // ========== SALON A1 (Tutor Ana Lopez) ==========
                // Matemáticas
                context.Materiales.Add(new Material 
                { 
                    Title = $"Matemáticas Semana {weekNum} - Álgebra y Aritmética", 
                    Description = $"Material completo de matemáticas para semana {weekNum}. Teoría, ejemplos resueltos y ejercicios propuestos con respuestas.", 
                    Week = weekNum,
                    SemanaId = semana.Id,
                    Curso = prof1.Cursos.FirstOrDefault() ?? null, 
                    Ciclo = ciclo, 
                    FileUrl = $"Ciclo_2025-II/Semana_{weekNum:D2}/SalonA1/matematicas_sem{weekNum}.pdf",
                    FileName = $"matematicas_semana_{weekNum}_A1.pdf",
                    FileSize = 1024 * (700 + weekNum * 15),
                    Salon = salon1, 
                    Tutor = tutor1,
                    TipoMaterial = TipoMaterial.PDF,
                    CreatedAt = DateTime.UtcNow 
                });

                // Video tutorial
                if (weekNum % 2 == 1) // Semanas impares
                {
                    context.Materiales.Add(new Material 
                    { 
                        Title = $"Video: Matemáticas Semana {weekNum} - Explicación Paso a Paso", 
                        Description = $"Tutorial en video explicando los temas de la semana {weekNum}. Duración aprox 30-40 minutos.", 
                        Week = weekNum,
                        SemanaId = semana.Id,
                        Curso = prof1.Cursos.FirstOrDefault() ?? null, 
                        Ciclo = ciclo, 
                        FileUrl = $"https://www.youtube.com/watch?v=matematicas_sem{weekNum}",
                        Salon = salon1, 
                        Tutor = tutor1,
                        TipoMaterial = TipoMaterial.Enlace,
                        CreatedAt = DateTime.UtcNow 
                    });
                }

                // Práctica
                context.Materiales.Add(new Material 
                { 
                    Title = $"Práctica Matemáticas Semana {weekNum}", 
                    Description = $"Ejercicios prácticos de matemáticas semana {weekNum}. Incluye problemas resueltos y propuestos.", 
                    Week = weekNum,
                    SemanaId = semana.Id,
                    Curso = prof1.Cursos.FirstOrDefault() ?? null, 
                    Ciclo = ciclo, 
                    FileUrl = $"Ciclo_2025-II/Semana_{weekNum:D2}/SalonA1/practica_matematicas_{weekNum}.pdf",
                    FileName = $"practica_matematicas_{weekNum}_A1.pdf",
                    FileSize = 1024 * 550,
                    Salon = salon1, 
                    Tutor = tutor1,
                    TipoMaterial = TipoMaterial.Documento,
                    CreatedAt = DateTime.UtcNow 
                });

                // ========== SALON A2 (Tutor Ana Lopez) ==========
                // Comunicación y Lenguaje
                context.Materiales.Add(new Material 
                { 
                    Title = $"Comunicación Semana {weekNum} - Comprensión Lectora", 
                    Description = $"Material de comunicación semana {weekNum}. Técnicas de comprensión, análisis de textos y ejercicios.", 
                    Week = weekNum,
                    SemanaId = semana.Id,
                    Curso = null, 
                    Ciclo = ciclo, 
                    FileUrl = $"Ciclo_2025-II/Semana_{weekNum:D2}/SalonA2/comunicacion_sem{weekNum}.pdf",
                    FileName = $"comunicacion_semana_{weekNum}_A2.pdf",
                    FileSize = 1024 * (650 + weekNum * 10),
                    Salon = salon2, 
                    Tutor = tutor1,
                    TipoMaterial = TipoMaterial.PDF,
                    CreatedAt = DateTime.UtcNow 
                });

                // Redacción
                context.Materiales.Add(new Material 
                { 
                    Title = $"Redacción y Ortografía Semana {weekNum}", 
                    Description = $"Guía de redacción y ortografía semana {weekNum}. Reglas, ejemplos y ejercicios prácticos.", 
                    Week = weekNum,
                    SemanaId = semana.Id,
                    Curso = null, 
                    Ciclo = ciclo, 
                    FileUrl = $"Ciclo_2025-II/Semana_{weekNum:D2}/SalonA2/redaccion_sem{weekNum}.pdf",
                    FileName = $"redaccion_semana_{weekNum}_A2.pdf",
                    FileSize = 1024 * 480,
                    Salon = salon2, 
                    Tutor = tutor1,
                    TipoMaterial = TipoMaterial.Documento,
                    CreatedAt = DateTime.UtcNow 
                });

                // ========== SALON B1 (Tutor Marcos Rojas - Inactivo pero con materiales) ==========
                // Física
                context.Materiales.Add(new Material 
                { 
                    Title = $"Física Semana {weekNum} - Mecánica y Cinemática", 
                    Description = $"Material de física semana {weekNum}. Teoría, fórmulas, problemas resueltos y propuestos.", 
                    Week = weekNum,
                    SemanaId = semana.Id,
                    Curso = prof2.Cursos.FirstOrDefault() ?? null, 
                    Ciclo = ciclo, 
                    FileUrl = $"Ciclo_2025-II/Semana_{weekNum:D2}/SalonB1/fisica_sem{weekNum}.pdf",
                    FileName = $"fisica_semana_{weekNum}_B1.pdf",
                    FileSize = 1024 * (750 + weekNum * 20),
                    Salon = salon3, 
                    Tutor = tutor2,
                    TipoMaterial = TipoMaterial.PDF,
                    CreatedAt = DateTime.UtcNow 
                });

                // Simuladores (semanas pares)
                if (weekNum % 2 == 0)
                {
                    context.Materiales.Add(new Material 
                    { 
                        Title = $"Simulador Física Semana {weekNum}", 
                        Description = $"Simulador interactivo de física para semana {weekNum}. Visualización de conceptos en tiempo real.", 
                        Week = weekNum,
                        SemanaId = semana.Id,
                        Curso = prof2.Cursos.FirstOrDefault() ?? null, 
                        Ciclo = ciclo, 
                        FileUrl = "https://phet.colorado.edu/sims/html/forces-and-motion-basics/latest/forces-and-motion-basics_es.html",
                        Salon = salon3, 
                        Tutor = tutor2,
                        TipoMaterial = TipoMaterial.Enlace,
                        CreatedAt = DateTime.UtcNow 
                    });
                }

                // ========== SALON B2 (Tutor Marcos Rojas) ==========
                // Química
                context.Materiales.Add(new Material 
                { 
                    Title = $"Química Semana {weekNum} - Teoría y Práctica", 
                    Description = $"Material de química semana {weekNum}. Conceptos teóricos, reacciones químicas y ejercicios.", 
                    Week = weekNum,
                    SemanaId = semana.Id,
                    Curso = prof2.Cursos.FirstOrDefault() ?? null, 
                    Ciclo = ciclo, 
                    FileUrl = $"Ciclo_2025-II/Semana_{weekNum:D2}/SalonB2/quimica_sem{weekNum}.pdf",
                    FileName = $"quimica_semana_{weekNum}_B2.pdf",
                    FileSize = 1024 * (680 + weekNum * 12),
                    Salon = salon4, 
                    Tutor = tutor2,
                    TipoMaterial = TipoMaterial.PDF,
                    CreatedAt = DateTime.UtcNow 
                });

                // Laboratorio virtual
                context.Materiales.Add(new Material 
                { 
                    Title = $"Laboratorio Virtual Química Semana {weekNum}", 
                    Description = $"Prácticas de laboratorio virtual de química semana {weekNum}. Experimentos simulados.", 
                    Week = weekNum,
                    SemanaId = semana.Id,
                    Curso = prof2.Cursos.FirstOrDefault() ?? null, 
                    Ciclo = ciclo, 
                    FileUrl = $"https://labster.com/simulations/quimica-semana-{weekNum}",
                    Salon = salon4, 
                    Tutor = tutor2,
                    TipoMaterial = TipoMaterial.Enlace,
                    CreatedAt = DateTime.UtcNow 
                });

                // ========== MATERIALES GENERALES (Todas las semanas) ==========
                // Cada 3 semanas: Material de repaso para todos los salones
                if (weekNum % 3 == 0)
                {
                    // Repaso Salón A1
                    context.Materiales.Add(new Material 
                    { 
                        Title = $"Repaso Acumulativo Semana {weekNum} - Salón A1", 
                        Description = $"Repaso de las últimas 3 semanas. Ejercicios integrados y evaluación.", 
                        Week = weekNum,
                        SemanaId = semana.Id,
                        Curso = prof1.Cursos.FirstOrDefault() ?? null, 
                        Ciclo = ciclo, 
                        FileUrl = $"Ciclo_2025-II/Semana_{weekNum:D2}/SalonA1/repaso_sem{weekNum}.pdf",
                        FileName = $"repaso_acumulativo_{weekNum}_A1.pdf",
                        FileSize = 1024 * 900,
                        Salon = salon1, 
                        Tutor = tutor1,
                        TipoMaterial = TipoMaterial.Documento,
                        CreatedAt = DateTime.UtcNow 
                    });

                    // Repaso Salón B1
                    context.Materiales.Add(new Material 
                    { 
                        Title = $"Repaso Acumulativo Semana {weekNum} - Salón B1", 
                        Description = $"Repaso de las últimas 3 semanas. Problemas integrados de física.", 
                        Week = weekNum,
                        SemanaId = semana.Id,
                        Curso = prof2.Cursos.FirstOrDefault() ?? null, 
                        Ciclo = ciclo, 
                        FileUrl = $"Ciclo_2025-II/Semana_{weekNum:D2}/SalonB1/repaso_sem{weekNum}.pdf",
                        FileName = $"repaso_acumulativo_{weekNum}_B1.pdf",
                        FileSize = 1024 * 850,
                        Salon = salon3, 
                        Tutor = tutor2,
                        TipoMaterial = TipoMaterial.Documento,
                        CreatedAt = DateTime.UtcNow 
                    });
                }
            }

            await context.SaveChangesAsync();
            logger?.LogInformation("? Materiales creados para TODOS los salones y TODAS las semanas del ciclo");

            // Agregar apoderados de ejemplo
            var apoderado1 = new Models.Apoderado
            {
                AlumnoId = alumnos[0].Id, // Carlos
                Nombre = "Roberto",
                Apellido = "Sanchez",
                DNI = "40123456",
                Email = "roberto.sanchez@example.com",
                Telefono = "987654321",
                Direccion = "Av. Los Olivos 456",
                Parentesco = "Padre",
                RecibeNotificaciones = true,
                FechaRegistro = DateTime.UtcNow
            };

            var apoderado2 = new Models.Apoderado
            {
                AlumnoId = alumnos[2].Id, // Pedro
                Nombre = "Carmen",
                Apellido = "Lopez",
                DNI = "40234567",
                Email = "carmen.lopez@example.com",
                Telefono = "987654322",
                Direccion = "Jr. Las Flores 789",
                Parentesco = "Madre",
                RecibeNotificaciones = true,
                FechaRegistro = DateTime.UtcNow
            };

            var apoderado3 = new Models.Apoderado
            {
                AlumnoId = alumnos[0].Id, // Carlos (segunda apoderada - madre)
                Nombre = "María",
                Apellido = "Sanchez",
                DNI = "40123457",
                Email = "maria.sanchez@example.com",
                Telefono = "987654323",
                Direccion = "Av. Los Olivos 456",
                Parentesco = "Madre",
                RecibeNotificaciones = true,
                FechaRegistro = DateTime.UtcNow
            };

            context.Apoderados.AddRange(apoderado1, apoderado2, apoderado3);
            await context.SaveChangesAsync();
            logger?.LogInformation("? Apoderados de ejemplo creados");

            // Crear usuarios Identity para apoderados
            foreach (var apod in new[] { apoderado1, apoderado2, apoderado3 })
            {
                var apoderadoUser = await userManager.FindByEmailAsync(apod.Email);
                if (apoderadoUser == null)
                {
                    apoderadoUser = new IdentityUser { Email = apod.Email, UserName = apod.Email, EmailConfirmed = true };
                    await userManager.CreateAsync(apoderadoUser, "Apoderado123!");
                    await userManager.AddToRoleAsync(apoderadoUser, "Apoderado");
                    logger?.LogInformation("Created apoderado user: {Email}", apod.Email);
                }
            }

            // Agregar notas de ejemplo para algunos alumnos
            var notasCarlos = new List<Nota>
            {
                new Nota
                {
                    AlumnoId = alumnos[0].Id,
                    CicloId = ciclo.Id,
                    SalonId = salon1.Id,
                    Materia = "Matemáticas",
                    Descripcion = "Examen Parcial",
                    Calificacion = 15.5m,
                    Peso = 1.0m,
                    TipoEvaluacion = TipoEvaluacion.ExamenParcial,
                    FechaEvaluacion = DateTime.UtcNow.AddDays(-10),
                    RegistradoPor = "tutor@academia.local",
                    FechaRegistro = DateTime.UtcNow.AddDays(-10),
                    IsActive = true
                },
                new Nota
                {
                    AlumnoId = alumnos[0].Id,
                    CicloId = ciclo.Id,
                    SalonId = salon1.Id,
                    Materia = "Comunicación",
                    Descripcion = "Trabajo Final",
                    Calificacion = 17.0m,
                    Peso = 1.0m,
                    TipoEvaluacion = TipoEvaluacion.Proyecto,
                    FechaEvaluacion = DateTime.UtcNow.AddDays(-5),
                    RegistradoPor = "tutor@academia.local",
                    FechaRegistro = DateTime.UtcNow.AddDays(-5),
                    IsActive = true
                },
                new Nota
                {
                    AlumnoId = alumnos[0].Id,
                    CicloId = ciclo.Id,
                    SalonId = salon1.Id,
                    Materia = "Ciencias",
                    Descripcion = "Práctica de Laboratorio",
                    Calificacion = 16.0m,
                    Peso = 1.0m,
                    TipoEvaluacion = TipoEvaluacion.Practica,
                    FechaEvaluacion = DateTime.UtcNow.AddDays(-3),
                    RegistradoPor = "tutor@academia.local",
                    FechaRegistro = DateTime.UtcNow.AddDays(-3),
                    IsActive = true
                }
            };

            context.Notas.AddRange(notasCarlos);
            
            // Calcular y actualizar promedio de Carlos
            var promedioCarlos = notasCarlos.Average(n => n.Calificacion);
            alumnos[0].PromedioGeneral = promedioCarlos;
            context.Alumnos.Update(alumnos[0]);
            
            await context.SaveChangesAsync();
            logger?.LogInformation("? Notas de ejemplo creadas y promedio calculado");
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
            ("marcos@academia.local", "Tutor", "Tutor123!"),
            ("roberto.sanchez@example.com", "Apoderado", "Apoderado123!"),
            ("carmen.lopez@example.com", "Apoderado", "Apoderado123!"),
            ("maria.sanchez@example.com", "Apoderado", "Apoderado123!")
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

    private static async Task EnsureConfiguracionPasarelaTableAsync(AcademicContext context, ILogger? logger)
    {
        var tableExists = await TableExistsAsync(context, "ConfiguracionPasarelas");
        if (tableExists) 
        {
            // Actualizar configuración por defecto a Culqi si no está configurado
            try
            {
                var config = await context.ConfiguracionPasarelas.FirstOrDefaultAsync();
                if (config == null)
                {
                    // Si no existe configuración, crear una nueva con Culqi
                    config = new ConfiguracionPasarela
                    {
                        PasarelaActiva = TipoPasarela.SinPasarela,
                        UltimaModificacion = DateTime.UtcNow,
                        ModificadoPor = "SYSTEM - Default Sin Pasarela"
                    };
                    context.ConfiguracionPasarelas.Add(config);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("? Configuración de pasarela creada con Sin Pasarela por defecto.");
                }
                else if (config.PasarelaActiva == TipoPasarela.Culqi)
                {
                    // Si es Culqi y está caída, cambiar a SinPasarela
                    config.PasarelaActiva = TipoPasarela.SinPasarela;
                    config.UltimaModificacion = DateTime.UtcNow;
                    config.ModificadoPor = "SYSTEM - Auto-config Sin Pasarela (Culqi caída)";
                    await context.SaveChangesAsync();
                    logger?.LogInformation("? Configuración de pasarela cambiada a Sin Pasarela por problemas con Culqi.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "No se pudo actualizar configuración de pasarela a Culqi");
            }
            return;
        }

        logger?.LogInformation("ConfiguracionPasarelas table missing — creating table.");

        var createSql = @"CREATE TABLE [ConfiguracionPasarelas](
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [PasarelaActiva] INT NOT NULL DEFAULT(0),
    [UltimaModificacion] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
    [ModificadoPor] NVARCHAR(256) NULL
);";
        await context.Database.ExecuteSqlRawAsync(createSql);

        // Insertar configuración por defecto (SinPasarela = 0)
        var insertDefault = "INSERT INTO [ConfiguracionPasarelas] (PasarelaActiva, UltimaModificacion, ModificadoPor) VALUES (0, GETUTCDATE(), 'SYSTEM - Default Sin Pasarela');";
        await context.Database.ExecuteSqlRawAsync(insertDefault);
        
        logger?.LogInformation("? ConfiguracionPasarela table created with Sin Pasarela as default payment gateway.");
    }

    private static async Task EnsureNotasTableAsync(AcademicContext context, ILogger? logger)
    {
        var tableExists = await TableExistsAsync(context, "Notas");
        if (tableExists) 
        {
            // Verificar si existe la columna PromedioGeneral en Alumnos
            var hasPromedioCol = await ColumnExistsAsync(context, "Alumnos", "PromedioGeneral");
            if (!hasPromedioCol)
            {
                logger?.LogInformation("Agregando columna PromedioGeneral a Alumnos...");
                await context.Database.ExecuteSqlRawAsync("ALTER TABLE [Alumnos] ADD [PromedioGeneral] decimal(5,2) NULL;");
                logger?.LogInformation("? Columna PromedioGeneral agregada.");
            }
            return;
        }

        logger?.LogInformation("Notas table missing — creating table.");

        var createSql = @"CREATE TABLE [Notas](
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [AlumnoId] INT NOT NULL,
    [CicloId] INT NOT NULL,
    [SalonId] INT NULL,
    [Materia] NVARCHAR(200) NOT NULL,
    [Descripcion] NVARCHAR(500) NULL,
    [Calificacion] DECIMAL(5,2) NOT NULL,
    [Peso] DECIMAL(5,2) NOT NULL DEFAULT(1.0),
    [TipoEvaluacion] INT NOT NULL DEFAULT(0),
    [FechaEvaluacion] DATETIME2 NOT NULL,
    [RegistradoPor] NVARCHAR(256) NULL,
    [FechaRegistro] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
    [Observaciones] NVARCHAR(MAX) NULL,
    [IsActive] BIT NOT NULL DEFAULT(1),
    CONSTRAINT FK_Notas_Alumnos FOREIGN KEY (AlumnoId) REFERENCES [Alumnos](Id) ON DELETE CASCADE,
    CONSTRAINT FK_Notas_Ciclos FOREIGN KEY (CicloId) REFERENCES [Ciclos](Id) ON DELETE CASCADE,
    CONSTRAINT FK_Notas_Salones FOREIGN KEY (SalonId) REFERENCES [Salones](Id) ON DELETE SET NULL
);";
        await context.Database.ExecuteSqlRawAsync(createSql);

        // Crear índices para mejorar performance
        var createIndexes = @"
CREATE INDEX IX_Notas_AlumnoId ON Notas(AlumnoId);
CREATE INDEX IX_Notas_CicloId ON Notas(CicloId);
CREATE INDEX IX_Notas_FechaEvaluacion ON Notas(FechaEvaluacion);
";
        await context.Database.ExecuteSqlRawAsync(createIndexes);

        // Agregar columna PromedioGeneral a Alumnos si no existe
        var hasPromedioGeneral = await ColumnExistsAsync(context, "Alumnos", "PromedioGeneral");
        if (!hasPromedioGeneral)
        {
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE [Alumnos] ADD [PromedioGeneral] decimal(5,2) NULL;");
        }
        
        logger?.LogInformation("? Notas table created with indexes and PromedioGeneral column added to Alumnos.");
    }

    private static async Task EnsureApoderadosTableAsync(AcademicContext context, ILogger? logger)
    {
        var tableExists = await TableExistsAsync(context, "Apoderados");
        if (tableExists) return;

        logger?.LogInformation("Apoderados table missing — creating table.");

        var createSql = @"CREATE TABLE [Apoderados](
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [AlumnoId] INT NOT NULL,
    [Nombre] NVARCHAR(100) NOT NULL,
    [Apellido] NVARCHAR(100) NOT NULL,
    [DNI] NVARCHAR(20) NOT NULL,
    [Email] NVARCHAR(256) NOT NULL,
    [Telefono] NVARCHAR(20) NOT NULL,
    [Direccion] NVARCHAR(200) NULL,
    [Parentesco] NVARCHAR(50) NULL,
    [RecibeNotificaciones] BIT NOT NULL DEFAULT(1),
    [FechaRegistro] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
    CONSTRAINT FK_Apoderados_Alumnos FOREIGN KEY (AlumnoId) REFERENCES [Alumnos](Id) ON DELETE CASCADE
);";
        await context.Database.ExecuteSqlRawAsync(createSql);
        
        logger?.LogInformation("? Apoderados table created.");
    }
}