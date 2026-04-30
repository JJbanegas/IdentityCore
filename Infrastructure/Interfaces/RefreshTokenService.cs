using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Interfaces;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IdentityCoreDbContext _dbContext;

    public RefreshTokenService(IdentityCoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> CreateAsync(string userId, int expirationDays = 7)
    {
        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();
        return token;
    }

    public async Task<RefreshToken?> GetValidAsync(string token)
    {
        return await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(r =>
                r.Token == token &&
                !r.IsRevoked &&
                r.ExpiresAt > DateTime.UtcNow);
    }

    public async Task RevokeAsync(string token)
    {
        var refreshToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == token);

        if (refreshToken != null)
        {
            refreshToken.IsRevoked = true;
            await _dbContext.SaveChangesAsync();
        }
    }
}

