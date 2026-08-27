namespace FinanceFlow.Domain.Entities;

public enum CreditCardInvoiceStatus
{
    Open = 1,
    Closed = 2,
    Paid = 3,
    Overdue = 4
}

public sealed class CreditCardInvoice
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public Guid CreditCardId { get; private set; }
    public DateTime ReferenceMonth { get; private set; }
    public DateTime ClosingDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public CreditCardInvoiceStatus Status { get; private set; } = CreditCardInvoiceStatus.Open;
    public DateTime? PaidAt { get; private set; }

    private CreditCardInvoice() { }

    public CreditCardInvoice(Guid userId, Guid creditCardId, DateTime referenceMonth, DateTime closingDate, DateTime dueDate)
    {
        UserId = userId;
        CreditCardId = creditCardId;
        ReferenceMonth = new DateTime(referenceMonth.Year, referenceMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        ClosingDate = closingDate.ToUniversalTime();
        DueDate = dueDate.ToUniversalTime();
    }

    public void Recalculate(decimal total, DateTime nowUtc)
    {
        TotalAmount = total;
        if (PaidAmount >= TotalAmount && TotalAmount > 0)
        {
            Status = CreditCardInvoiceStatus.Paid;
            return;
        }
        if (nowUtc >= ClosingDate && Status == CreditCardInvoiceStatus.Open) Status = CreditCardInvoiceStatus.Closed;
        if (nowUtc > DueDate && PaidAmount < TotalAmount) Status = CreditCardInvoiceStatus.Overdue;
    }

    public void Pay(decimal amount, DateTime paidAtUtc)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        PaidAmount = Math.Min(TotalAmount, PaidAmount + amount);
        PaidAt = paidAtUtc.ToUniversalTime();
        Status = PaidAmount >= TotalAmount && TotalAmount > 0 ? CreditCardInvoiceStatus.Paid : CreditCardInvoiceStatus.Closed;
    }
}
