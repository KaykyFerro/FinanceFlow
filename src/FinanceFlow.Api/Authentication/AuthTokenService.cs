using System.Security.Cryptography;
using FinanceFlow.Infrastructure.Data;
using FinanceFlow.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceFlow.Api.Authentication;

public sealed class AuthTokenService(FinanceFlowDbContext db)
{
    public async Task<string> CreateAsync(Guid userId, AuthTokenType type, TimeSpan lifetime, CancellationToken cancellationToken)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        db.AuthTokens.Add(new AuthToken
        {
            UserId = userId,
            Type = type,
            TokenHash = TokenService.HashToken(rawToken),
            ExpiresAtUtc = DateTime.UtcNow.Add(lifetime)
        });
        await db.SaveChangesAsync(cancellationToken);
        return rawToken;
    }

    public async Task<AuthToken?> ConsumeAsync(string rawToken, AuthTokenType type, CancellationToken cancellationToken)
    {
        var hash = TokenService.HashToken(rawToken);
        var token = await db.AuthTokens.SingleOrDefaultAsync(x => x.TokenHash == hash && x.Type == type, cancellationToken);
        if (token is null || token.UsedAtUtc is not null || token.ExpiresAtUtc <= DateTime.UtcNow) return null;
        token.UsedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return token;
    }
}
