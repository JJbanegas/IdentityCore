using Infrastructure.Entities;

namespace Infrastructure.Interfaces;

public interface IRefreshTokenService
{
    /// <summary>Crea y persiste un nuevo refresh token para el usuario dado.</summary>
    Task<string> CreateAsync(string userId, int expirationDays = 7);

    /// <summary>Devuelve el RefreshToken si es válido (no revocado, no expirado), o null.</summary>
    Task<RefreshToken?> GetValidAsync(string token);

    /// <summary>Marca el token como revocado.</summary>
    Task RevokeAsync(string token);
}

