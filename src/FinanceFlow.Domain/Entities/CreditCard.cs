namespace FinanceFlow.Domain.Entities;

public sealed class CreditCard
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string Institution { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? LastFourDigits { get; private set; }
    public decimal CreditLimit { get; private set; }
    public int ClosingDay { get; private set; }
    public int DueDay { get; private set; }
    public bool Active { get; private set; } = true;

    private CreditCard() { }

    public CreditCard(Guid userId, string institution, string name, decimal creditLimit, int closingDay, int dueDay, string? lastFourDigits = null)
    {
        UserId = userId;
        Institution = institution.Trim();
        Name = name.Trim();
        CreditLimit = creditLimit;
        ClosingDay = closingDay;
        DueDay = dueDay;
        LastFourDigits = NormalizeLastFour(lastFourDigits);
    }

    public void Update(string institution, string name, decimal creditLimit, int closingDay, int dueDay, string? lastFourDigits, bool active)
    {
        Institution = institution.Trim();
        Name = name.Trim();
        CreditLimit = creditLimit;
        ClosingDay = closingDay;
        DueDay = dueDay;
        LastFourDigits = NormalizeLastFour(lastFourDigits);
        Active = active;
    }

    private static string? NormalizeLastFour(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length <= 4 ? digits : digits[^4..];
    }
}
