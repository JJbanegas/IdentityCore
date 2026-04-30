using Application.Commands.Partners.v1;
using Application.Mappings;
using IdentityCore.Services;
using Infrastructure;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace IdentityCore.Helpers;

public static class ConfigurationExtensions
{
    public static void SetConfigurationExtensions(IServiceCollection service, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey no configurado");

        service
            .AddDbContext<IdentityCoreDbContext>(options =>
                options.UseSqlServer(Environment.GetEnvironmentVariable("connectionstring")))
            .AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new() { Title = "IdentityCore API", Version = "v1" });
                options.SwaggerDoc("v2", new() { Title = "IdentityCore API", Version = "v2" });

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
            .AddMediatR(cfg => { cfg.RegisterServicesFromAssemblyContaining<PartnerCreateCommand>(); });

        service.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        service.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
            options.AddPolicy("AdminOrPartner", policy => policy.RequireRole("Admin", "Partner"));
            // Ejemplo para permisos granulares (futuro):
            // options.AddPolicy("CanReadUsers", policy => policy.RequireClaim("permissions", "user:read"));
        });
    }
}