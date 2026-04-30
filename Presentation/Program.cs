using Company.BuildingBlocks.Contracts.Extensions;
using Company.BuildingBlocks.Contracts.Models;
using IdentityCore.Helpers;
using Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using ConfigurationExtensions = IdentityCore.Helpers.ConfigurationExtensions;

var builder = WebApplication.CreateBuilder(args);

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
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityCoreDbContext>();
        dbContext.Database.Migrate();
    }
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStatusCodePages(async statusCodeContext =>
{
    var httpContext = statusCodeContext.HttpContext;
    if (!httpContext.Request.Path.StartsWithSegments("/api"))
        return;

    var status = httpContext.Response.StatusCode;
    if (status is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden)
    {
        var detail = status == StatusCodes.Status401Unauthorized
            ? "Debes enviar un bearer token valido para acceder a este recurso."
            : "No tienes permisos suficientes para acceder a este recurso.";

        if (httpContext.Response.HasStarted)
            return;

        object payload;

        // Intentamos usar el helper central del paquete compartido para mantener consistencia.
        var responseType = typeof(ApiResponse<string>);
        var errorFactory = responseType.GetMethod(
            "ErrorResponse",
            BindingFlags.Public | BindingFlags.Static,
            [typeof(string)]);

        if (errorFactory != null)
        {
            payload = errorFactory.Invoke(null, [detail])!;
        }
        else
        {
            // Fallback seguro si el paquete no expone ErrorResponse(string).
            payload = new
            {
                success = false,
                message = detail,
                data = (string?)null,
                statusCode = status
            };
        }

        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(payload);
    }
});
app.RegisterEndpointDefinitions();
app.Run();
