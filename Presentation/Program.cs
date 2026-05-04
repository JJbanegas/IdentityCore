using Company.BuildingBlocks.Contracts.Extensions;
using IdentityCore.Helpers;
using Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ConfigurationExtensions = IdentityCore.Helpers.ConfigurationExtensions;

var builder = WebApplication.CreateBuilder(args);
var runMigrationsOnStartup = builder.Configuration.GetValue<bool>("Database:RunMigrationsOnStartup");

// Pasa IConfiguration para que ConfigurationExtensions configure JWT Bearer y Swagger
ConfigurationExtensions.SetConfigurationExtensions(builder.Services, builder.Configuration);

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<IdentityCoreDbContext>()
    .AddDefaultTokenProviders();

// En APIs, evitamos redirects HTML a /Account/Login y forzamos respuestas HTTP (401/403)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

// AddIdentity puede cambiar el esquema por defecto a cookie; lo re-forzamos a JWT
builder.Services.PostConfigure<AuthenticationOptions>(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
});

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
});

builder.Services.AddBuildingBlocks();

var app = builder.Build();

// Seed de roles en TODOS los entornos (idempotente: solo crea si no existe)
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { "Admin", "User", "Partner" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

app.UseBuildingBlocks();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "IdentityCore API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "IdentityCore API v2");
    });
}

if (runMigrationsOnStartup)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<IdentityCoreDbContext>();
    dbContext.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseCors("DefaultCors");
app.UseAuthentication();
app.UseAuthorization();
app.RegisterEndpointDefinitions();
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "IdentityCore", utc = DateTime.UtcNow }));
app.Run();
