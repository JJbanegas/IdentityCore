using Application.Commands.Partners.v1;
using Application.Mappings;
using Company.BuildingBlocks.Contracts.Extensions;
using IdentityCore.Services;
using Infrastructure;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace IdentityCore.Helpers;

public static class ConfigurationExtensions
{
    public static void SetConfigurationExtensions(IServiceCollection service, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? Environment.GetEnvironmentVariable("connectionstring")
                               ?? throw new InvalidOperationException("No se encontro configuracion de base de datos. Configura ConnectionStrings:DefaultConnection o la variable connectionstring.");
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        service.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", policy =>
            {
                if (allowedOrigins.Length == 0)
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    return;
                }

                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        service
            .AddDbContext<IdentityCoreDbContext>(options =>
                options.UseSqlServer(connectionString))
            .AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new() {Title = "IdentityCore API", Version = "v1"});
                options.SwaggerDoc("v2", new() {Title = "IdentityCore API", Version = "v2"});

                // Soporte JWT Bearer en Swagger UI
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Ingresa el token JWT. Ejemplo: 'Bearer eyJhbGci...'"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            })
            .AddEndpointsApiExplorer()
            .AddAutoMapper(typeof(IdentityMapping))
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped(typeof(IRepository<>), typeof(Repository<>))
            .AddScoped<IRefreshTokenService, RefreshTokenService>()
            .AddScoped<IJwtTokenService, JwtTokenService>()
            .AddMediatR(cfg => { cfg.RegisterServicesFromAssemblyContaining<PartnerCreateCommand>(); })
            .AddBuildingBlocksJwtAuth(configuration, options =>
            {
                options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
                options.AddPolicy("UserOnly", p => p.RequireRole("User"));
                options.AddPolicy("AdminOrPartner", p => p.RequireRole("Admin", "Partner"));
            });
    }
}