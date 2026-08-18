namespace FinanceFlow.Domain.Entities;

public sealed class Budget
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public Guid CategoryId { get; private set; }
    public DateTime Month { get; private set; }
    public decimal LimitAmount { get; private set; }

    private Budget() { }
    public Budget(Guid userId, Guid categoryId, DateTime month, decimal limitAmount)
    {
        UserId = userId;
        CategoryId = categoryId;
        Month = new DateTime(month.Year, month.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        LimitAmount = limitAmount;
    }
    public void Update(Guid categoryId, DateTime month, decimal limitAmount)
    {
        CategoryId = categoryId;
        Month = new DateTime(month.Year, month.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        LimitAmount = limitAmount;
    }
}
