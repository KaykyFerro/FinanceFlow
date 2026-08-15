namespace FinanceFlow.Domain.Entities;

public enum AccountType
{
    Checking,
    Savings,
    Wallet,
    Cash,
    Investment
}

public enum YieldType
{
    None,
    Savings,
    CdiPercentage,
    FixedAnnualRate,
    IpcaPlus
}

public sealed class Account
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string Institution { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public AccountType Type { get; private set; }
    public decimal Balance { get; private set; }
    public YieldType YieldType { get; private set; }
    public decimal? YieldPercentage { get; private set; }

    private Account() { }

    public Account(Guid userId, string institution, string name, AccountType type, decimal balance = 0)
    {
        UserId = userId;
        Institution = institution;
        Name = name;
        Type = type;
        Balance = balance;
    }

    public void ConfigureYield(YieldType yieldType, decimal? percentage = null)
    {
        YieldType = yieldType;
        YieldPercentage = percentage;
    }
}
