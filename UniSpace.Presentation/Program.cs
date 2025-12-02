using EVAuctionTrader.Presentation.Helper;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using UniSpace.Presentation.Architecture;

var builder = WebApplication.CreateBuilder(args);

builder.Services.SetupIocContainer();
builder.Configuration
  .AddJsonFile("appsettings.json", true, true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    // Configure authorization for specific pages
    options.Conventions.AuthorizePage("/Dashboard");
    options.Conventions.AuthorizeFolder("/Admin", "AdminPolicy");
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToFolder("/Auth");
});

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.WebHost.UseUrls("http://localhost:5000");
builder.Services.AddDistributedMemoryCache();

// Configure data protection key persistence
var dataProtectionPath = builder.Configuration["DataProtection:KeyPath"]
 ?? Environment.GetEnvironmentVariable("DATA_PROTECTION_KEY_PATH")
        ?? "/keys";

try
{
    if (!Directory.Exists(dataProtectionPath))
    {
        Directory.CreateDirectory(dataProtectionPath);
    }

    builder.Services.AddDataProtection()
     .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
        .SetApplicationName("UniSpace");
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: could not configure persistent data protection keys at '{dataProtectionPath}': {ex.Message}");
}

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Apply database migrations and seed data
var logger = app.Services.GetRequiredService<ILogger<Program>>();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UniSpace.Domain.UniSpaceDbContext>();

    try
    {
        logger.LogInformation("=== Database Initialization Started ===");

        // Kiểm tra xem database có tồn tại không
        bool databaseExists = dbContext.Database.CanConnect();

        if (databaseExists)
        {
            // Kiểm tra xem có migration nào chưa được apply không
            var pendingMigrations = dbContext.Database.GetPendingMigrations();

            if (pendingMigrations.Any())
            {
                logger.LogInformation("⚠ Database exists but has pending migrations. Running migrations...");
                app.ApplyMigrations(logger);
            }
            else
            {
                logger.LogInformation("✓ Database already exists and is up to date. Skipping migrations.");
            }
        }
        else
        {
            // Database chưa tồn tại, cần chạy migration
            logger.LogInformation("⚠ Database does not exist. Creating and running migrations...");
            app.ApplyMigrations(logger);
        }

        // Seed initial data after migrations
        logger.LogInformation("🌱 Seeding initial data...");

        await DbSeeder.SeedUsersAsync(dbContext);
        logger.LogInformation("✓ Users seeded successfully");

        await DbSeeder.SeedCampusesAsync(dbContext);
        logger.LogInformation("✓ Campuses seeded successfully");

        await DbSeeder.SeedRoomsAsync(dbContext);
        logger.LogInformation("✓ Rooms seeded successfully");

        logger.LogInformation("=== Database Initialization Completed ===");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error during database initialization");

        // Try to recover by running migrations and seeding
        try
        {
            logger.LogWarning("Attempting recovery: Running migrations...");
            app.ApplyMigrations(logger);

            logger.LogWarning("Attempting recovery: Seeding data...");
            await DbSeeder.SeedUsersAsync(dbContext);
            await DbSeeder.SeedCampusesAsync(dbContext);
            await DbSeeder.SeedRoomsAsync(dbContext);

            logger.LogInformation("✓ Recovery successful");
        }
        catch (Exception recoveryEx)
        {
            logger.LogError(recoveryEx, "❌ Recovery failed - Manual intervention may be required");
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();