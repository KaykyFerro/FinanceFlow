namespace FinanceFlow.Domain.Entities;

public sealed class CreditCardPurchase
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public Guid CreditCardId { get; private set; }
    public Guid? CategoryId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public int Installments { get; private set; }
    public DateTime PurchaseDate { get; private set; }
    public DateTime FirstInvoiceMonth { get; private set; }
    public string? Notes { get; private set; }
    public bool Confirmed { get; private set; } = true;

    private CreditCardPurchase() { }

    public CreditCardPurchase(Guid userId, Guid creditCardId, Guid? categoryId, string description, decimal totalAmount, int installments, DateTime purchaseDate, DateTime firstInvoiceMonth, string? notes = null)
    {
        UserId = userId;
        CreditCardId = creditCardId;
        CategoryId = categoryId;
        Description = description.Trim();
        TotalAmount = totalAmount;
        Installments = installments;
        PurchaseDate = purchaseDate.ToUniversalTime();
        FirstInvoiceMonth = new DateTime(firstInvoiceMonth.Year, firstInvoiceMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public decimal InstallmentAmount => Installments <= 0 ? TotalAmount : Math.Round(TotalAmount / Installments, 2, MidpointRounding.AwayFromZero);

    public DateTime GetInvoiceMonth(int installmentNumber)
    {
        if (installmentNumber < 1 || installmentNumber > Installments) throw new ArgumentOutOfRangeException(nameof(installmentNumber));
        return FirstInvoiceMonth.AddMonths(installmentNumber - 1);
    }
}
