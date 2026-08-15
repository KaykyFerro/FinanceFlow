namespace FinanceFlow.Infrastructure.Entities;

public enum AuthTokenType
{
    EmailVerification,
    PasswordReset
}

public sealed class AuthToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public AuthTokenType Type { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UsedAtUtc { get; set; }
}
