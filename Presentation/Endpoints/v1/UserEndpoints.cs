using Application.Commands.Partners.v1;
using Application.Queries.Partners.v1;
using Application.ViewModels.Users;
using Company.BuildingBlocks.Contracts.Models;
using IdentityCore.Helpers;
using IdentityCore.Services;
using Infrastructure.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityCore.Endpoints.v1;

public class UserEndpoints : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/user")
            .WithOpenApi()
            .WithTags("User")
            .WithMetadata(new ApiExplorerSettingsAttribute {GroupName = "v1"})
            .WithGroupName("v1");

        // ─── REGISTER ────────────────────────────────────────────────────────
        group.MapPost("/Register",
            async ([FromServices] IMediator mediator, [FromBody] UserRegisterRequestViewModel model) =>
            {
                var result = await mediator.Send(new UserRegisterCommand(model));
                return Results.Created("/Register",
                    ApiResponse<string>.SuccessResponse(result, "Usuario creado exitosamente"));
            });

        // ─── LOGIN ────────────────────────────────────────────────────────────
        group.MapPost("/Login",
            async (
                HttpContext ctx,
                [FromServices] IMediator mediator,
                [FromServices] IJwtTokenService jwtService,
                [FromServices] IRefreshTokenService refreshTokenService,
                [FromServices] IConfiguration config,
                [FromServices] IWebHostEnvironment env,
                [FromBody] UserLoginRequestViewModel model) =>
            {
                var result = await mediator.Send(new UserLoginCommand(model));

                var identityUser = new IdentityUser
                {
                    Id = result.UserId,
                    UserName = result.UserName,
                    Email = result.Email
                };

                var accessToken = jwtService.GenerateToken(identityUser, result.Roles);
                var expirationMinutes = int.Parse(
                    config.GetSection("JwtSettings")["ExpirationMinutes"] ?? "60");

                var refreshToken = await refreshTokenService.CreateAsync(result.UserId);

                ctx.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict,
                    Secure = !env.IsDevelopment(), // false en dev (HTTP), true en prod (HTTPS)
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

                return Results.Ok(ApiResponse<UserLoginResponseViewModel>.SuccessResponse(
                    new UserLoginResponseViewModel
                    {
                        AccessToken = accessToken,
                        TokenType = "Bearer",
                        ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
                        UserId = result.UserId,
                        UserName = result.UserName,
                        Email = result.Email,
                        Roles = result.Roles.ToList()
                    }, "Login exitoso"));
            });

        // ─── REFRESH TOKEN ───────────────────────────────────────────────────
        group.MapPost("/Refresh",
            async (
                HttpContext ctx,
                [FromServices] IRefreshTokenService refreshTokenService,
                [FromServices] IJwtTokenService jwtService,
                [FromServices] UserManager<IdentityUser> userManager,
                [FromServices] IConfiguration config,
                [FromServices] IWebHostEnvironment env) =>
            {
                var tokenString = ctx.Request.Cookies["refreshToken"];
                if (string.IsNullOrEmpty(tokenString))
                    return Results.Problem(
                        title: "Token no encontrado",
                        detail: "No se encontró el refresh token en la cookie.",
                        statusCode: StatusCodes.Status401Unauthorized);

                var refreshToken = await refreshTokenService.GetValidAsync(tokenString);
                if (refreshToken == null)
                    return Results.Problem(
                        title: "Token inválido",
                        detail: "El refresh token es inválido o ha expirado.",
                        statusCode: StatusCodes.Status401Unauthorized);

                var user = await userManager.FindByIdAsync(refreshToken.UserId);
                if (user == null)
                    return Results.Problem(
                        title: "Usuario no encontrado",
                        detail: "El usuario asociado al token ya no existe.",
                        statusCode: StatusCodes.Status401Unauthorized);

                var roles = await userManager.GetRolesAsync(user);

                // Rotación: se revoca el token anterior y se emite uno nuevo
                await refreshTokenService.RevokeAsync(tokenString);
                var newRefreshToken = await refreshTokenService.CreateAsync(user.Id);
                var newAccessToken = jwtService.GenerateToken(user, roles);
                var expirationMinutes = int.Parse(
                    config.GetSection("JwtSettings")["ExpirationMinutes"] ?? "60");

                ctx.Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict,
                    Secure = !env.IsDevelopment(),
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

                return Results.Ok(ApiResponse<UserLoginResponseViewModel>.SuccessResponse(
                    new UserLoginResponseViewModel
                    {
                        AccessToken = newAccessToken,
                        TokenType = "Bearer",
                        ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
                        UserId = user.Id,
                        UserName = user.UserName ?? string.Empty,
                        Email = user.Email ?? string.Empty,
                        Roles = roles.ToList()
                    }, "Token renovado exitosamente"));
            });

        // ─── LOGOUT ───────────────────────────────────────────────────────────
        group.MapPost("/Logout",
            async (HttpContext ctx, [FromServices] IRefreshTokenService refreshTokenService) =>
            {
                var tokenString = ctx.Request.Cookies["refreshToken"];
                if (!string.IsNullOrEmpty(tokenString))
                    await refreshTokenService.RevokeAsync(tokenString);

                ctx.Response.Cookies.Delete("refreshToken");

                return Results.Ok(ApiResponse<string>.SuccessResponse(string.Empty, "Sesión cerrada exitosamente"));
            });

        // ─── GET USERS ───────────────────────────────────────────
        group.MapGet("/GetUsers",
            async ([FromServices] IMediator mediator) =>
            {
                var result = await mediator.Send(new UserGetAllQuery());
                return Results.Ok(ApiResponse<List<UserGetAllResponseViewModel>>.SuccessResponse(
                    result, "Usuarios obtenidos exitosamente"));
            })
            .Produces<ApiResponse<List<UserGetAllResponseViewModel>>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization("AdminOnly");
    }
}