using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FinanceFlow.Domain.Entities;
using FinanceFlow.Infrastructure.Data;
using FinanceFlow.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FinanceFlow.Api.Authentication;

public sealed class TokenService(FinanceFlowDbContext db, IOptions<JwtOptions> options)
{
    private readonly JwtOptions _options = options.Value;

    public async Task<AuthResponse> IssueAsync(User user, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_options.AccessTokenMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var jwt = new JwtSecurityToken(_options.Issuer, _options.Audience, claims, now, expires, credentials);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
        var rawRefreshToken = CreateRandomToken();

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(rawRefreshToken),
            ExpiresAtUtc = now.AddDays(_options.RefreshTokenDays)
        });

        await db.SaveChangesAsync(cancellationToken);
        return new AuthResponse(accessToken, rawRefreshToken, expires, ToResponse(user));
    }

    public async Task<AuthResponse?> RotateAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var hash = HashToken(refreshToken);
        var stored = await db.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
        if (stored is null || !stored.IsActive) return null;

        var user = await db.Users.SingleOrDefaultAsync(x => x.Id == stored.UserId, cancellationToken);
        if (user is null) return null;

        stored.RevokedAtUtc = DateTime.UtcNow;
        return await IssueAsync(user, cancellationToken);
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var hash = HashToken(refreshToken);
        var stored = await db.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
        if (stored is not null) stored.RevokedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public static string CreateRandomToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    public static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    public static UserResponse ToResponse(User user) => new(user.Id, user.Name, user.Email, user.EmailConfirmed, user.CreatedAtUtc, user.LastLoginAtUtc);
}
