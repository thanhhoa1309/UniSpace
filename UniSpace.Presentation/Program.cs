using Microsoft.AspNetCore.DataProtection;
using System.IdentityModel.Tokens.Jwt;
using UniSpace.Presentation.Architecture;

var builder = WebApplication.CreateBuilder(args);

builder.Services.SetupIocContainer();
builder.Configuration
  .AddJsonFile("appsettings.json", true, true)
    .AddEnvironmentVariables();



// Add services to the container.
builder.Services.AddRazorPages();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.WebHost.UseUrls("http://0.0.0.0:5000");
builder.Services.AddDistributedMemoryCache();
// Configure data protection key persistence. Keys will be written to a folder
// mounted into the container (e.g. host ./data/keys -> container /keys).
// The path can be overridden via configuration: DataProtection:KeyPath or env DATA_PROTECTION_KEY_PATH.
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
        .SetApplicationName("EVAuctionTrader");
}
catch (Exception ex)
{
    // If key storage cannot be configured (e.g., missing permissions), continue with default ephemeral keys but warn in logs at runtime.
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/Home/LandingPage"));

app.MapRazorPages();

app.Run();
