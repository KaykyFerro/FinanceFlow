namespace FinanceFlow.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool EmailConfirmed { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? LastLoginAtUtc { get; private set; }

    private User() { }

    public User(string name, string email, string passwordHash)
    {
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
    }

    public void ConfirmEmail() => EmailConfirmed = true;
    public void RegisterLogin() => LastLoginAtUtc = DateTime.UtcNow;
    public void ChangePasswordHash(string passwordHash) => PasswordHash = passwordHash;
    public void UpdateProfile(string name, string email)
    {
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
    }
}
