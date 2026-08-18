namespace FinanceFlow.Domain.Entities;

public enum TransactionType { Income = 1, Expense = 2 }

public sealed class Transaction
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid? CategoryId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public DateTime Date { get; private set; }
    public string? Notes { get; private set; }
    public bool Confirmed { get; private set; } = true;

    private Transaction() { }

    public Transaction(Guid userId, Guid accountId, Guid? categoryId, string description, decimal amount, TransactionType type, DateTime date, string? notes = null)
    {
        UserId = userId;
        AccountId = accountId;
        CategoryId = categoryId;
        Description = description.Trim();
        Amount = amount;
        Type = type;
        Date = date;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public void Update(Guid accountId, Guid? categoryId, string description, decimal amount, TransactionType type, DateTime date, string? notes)
    {
        AccountId = accountId;
        CategoryId = categoryId;
        Description = description.Trim();
        Amount = amount;
        Type = type;
        Date = date;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
