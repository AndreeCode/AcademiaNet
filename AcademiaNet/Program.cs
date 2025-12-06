using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Academic.Data;
using Academic.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// Connection string: DefaultConnection from appsettings
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Server=localhost;Database=academic;User Id=sa;Password=pass;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True";

builder.Services.AddDbContext<AcademicContext>(options =>
    options.UseSqlServer(connectionString));

// Identity with roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<AcademicContext>()
    .AddDefaultTokenProviders();

builder.Services.AddRazorPages();

builder.Services.Configure<MercadoPagoOptions>(builder.Configuration.GetSection("MercadoPago"));
builder.Services.AddScoped<MercadoPagoService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Run DB connection check, migrations and seeder at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<AcademicContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        logger.LogInformation("Checking database connectivity...");

        var canConnect = await context.Database.CanConnectAsync();
        if (!canConnect)
        {
            logger.LogError("Could not connect to the database. Please check the connection string and ensure the database server is reachable.");
            Console.Error.WriteLine("ERROR: Could not connect to the database. Aborting startup.");
            Environment.Exit(1);
        }

        try
        {
            logger.LogInformation("Applying database migrations...");
            await DbInitializer.ApplyMigrationsAsync(context, logger);
            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations.");
            Console.Error.WriteLine($"ERROR: Migration failed: {ex.Message}");
            Environment.Exit(2);
        }

        try
        {
            logger.LogInformation("Executing database seeders...");
            await DbInitializer.SeedAsync(context, userManager, roleManager);
            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            Console.Error.WriteLine($"ERROR: Seeding failed: {ex.Message}");
            Environment.Exit(3);
        }
    }
    catch (Exception ex)
    {
        var logger2 = services.GetRequiredService<ILogger<Program>>();
        logger2.LogError(ex, "Unexpected error during startup database initialization.");
        Console.Error.WriteLine($"ERROR: Unexpected startup error: {ex.Message}");
        Environment.Exit(4);
    }
}

app.Run();
