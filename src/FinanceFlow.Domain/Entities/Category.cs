namespace FinanceFlow.Domain.Entities;

public sealed class Category
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Color { get; private set; } = "#0aa579";
    public bool IsIncome { get; private set; }

    private Category() { }
    public Category(Guid userId, string name, string color = "#0aa579", bool isIncome = false)
    {
        UserId = userId;
        Name = name.Trim();
        Color = string.IsNullOrWhiteSpace(color) ? "#0aa579" : color;
        IsIncome = isIncome;
    }
    public void Update(string name, string color, bool isIncome)
    {
        Name = name.Trim();
        Color = string.IsNullOrWhiteSpace(color) ? Color : color;
        IsIncome = isIncome;
    }
}
