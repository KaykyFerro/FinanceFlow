using System.Security.Claims;
using FinanceFlow.Domain.Entities;
using FinanceFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/credit-cards")]
public sealed class CreditCardsController(FinanceFlowDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var userId = GetUserId();
        var cards = await db.CreditCards.AsNoTracking().Where(x => x.UserId == userId && x.Active).OrderBy(x => x.Name).ToListAsync(ct);
        var result = new List<object>();
        foreach (var card in cards)
        {
            var invoices = await BuildInvoices(card, ct);
            var current = invoices.FirstOrDefault(IsCurrentInvoice) ?? invoices.FirstOrDefault(x => x.ReferenceMonth >= Month(DateTime.UtcNow));
            var used = current?.TotalAmount - current.PaidAmount ?? 0m;
            result.Add(new
            {
                card.Id, card.Institution, card.Name, card.LastFourDigits, card.CreditLimit, card.ClosingDay, card.DueDay,
                usedLimit = Math.Max(0, used), availableLimit = Math.Max(0, card.CreditLimit - used),
                currentInvoice = current
            });
        }
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var card = await db.CreditCards.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.UserId == GetUserId(), ct);
        if (card is null) return NotFound();
        var invoices = await BuildInvoices(card, ct);
        return Ok(new
        {
            card,
            invoices,
            usedLimit = invoices.Where(x => x.ReferenceMonth >= Month(DateTime.UtcNow) && x.Status != CreditCardInvoiceStatus.Paid).Sum(x => x.TotalAmount - x.PaidAmount),
            availableLimit = Math.Max(0, card.CreditLimit - invoices.Where(x => x.ReferenceMonth >= Month(DateTime.UtcNow) && x.Status != CreditCardInvoiceStatus.Paid).Sum(x => x.TotalAmount - x.PaidAmount))
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CardRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(request.Name) || request.CreditLimit < 0) return BadRequest(new { message = "Nome e limite válido são obrigatórios." });
        if (!ValidDay(request.ClosingDay) || !ValidDay(request.DueDay)) return BadRequest(new { message = "Dias de fechamento e vencimento devem estar entre 1 e 28." });
        var card = new CreditCard(userId, request.Institution ?? "", request.Name, request.CreditLimit, request.ClosingDay, request.DueDay, request.LastFourDigits);
        db.CreditCards.Add(card);
        await db.SaveChangesAsync(ct);
        return Created($"/api/credit-cards/{card.Id}", card);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CardRequest request, CancellationToken ct)
    {
        var card = await db.CreditCards.SingleOrDefaultAsync(x => x.Id == id && x.UserId == GetUserId(), ct);
        if (card is null) return NotFound();
        if (string.IsNullOrWhiteSpace(request.Name) || request.CreditLimit < 0 || !ValidDay(request.ClosingDay) || !ValidDay(request.DueDay)) return BadRequest(new { message = "Dados inválidos." });
        card.Update(request.Institution ?? "", request.Name, request.CreditLimit, request.ClosingDay, request.DueDay, request.LastFourDigits, request.Active);
        await db.SaveChangesAsync(ct);
        return Ok(card);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var card = await db.CreditCards.SingleOrDefaultAsync(x => x.Id == id && x.UserId == GetUserId(), ct);
        if (card is null) return NotFound();
        card.Update(card.Institution, card.Name, card.CreditLimit, card.ClosingDay, card.DueDay, card.LastFourDigits, false);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/invoices")]
    public async Task<IActionResult> Invoices(Guid id, CancellationToken ct)
    {
        var card = await db.CreditCards.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.UserId == GetUserId(), ct);
        if (card is null) return NotFound();
        return Ok(await BuildInvoices(card, ct));
    }

    [HttpPost("{id:guid}/purchases")]
    public async Task<IActionResult> Purchase(Guid id, PurchaseRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var card = await db.CreditCards.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId && x.Active, ct);
        if (card is null) return NotFound();
        if (request.Amount <= 0 || request.Installments is < 1 or > 120 || string.IsNullOrWhiteSpace(request.Description)) return BadRequest(new { message = "Descrição, valor e parcelas válidos são obrigatórios." });
        if (request.CategoryId is not null && !await db.Categories.AnyAsync(x => x.Id == request.CategoryId && x.UserId == userId, ct)) return BadRequest(new { message = "Categoria inválida." });

        var firstMonth = request.FirstInvoiceMonth.HasValue ? Month(request.FirstInvoiceMonth.Value) : CalculateInvoiceMonth(card, request.PurchaseDate.ToUniversalTime());
        var purchase = new CreditCardPurchase(userId, id, request.CategoryId, request.Description, request.Amount, request.Installments, request.PurchaseDate, firstMonth, request.Notes);
        db.CreditCardPurchases.Add(purchase);
        await db.SaveChangesAsync(ct);
        return Created($"/api/credit-cards/{id}/purchases/{purchase.Id}", new { purchase.Id, purchase.Description, purchase.TotalAmount, purchase.Installments, installmentAmount = purchase.InstallmentAmount, purchase.PurchaseDate, purchase.FirstInvoiceMonth });
    }

    [HttpPost("{id:guid}/invoices/{invoiceId:guid}/pay")]
    public async Task<IActionResult> PayInvoice(Guid id, Guid invoiceId, PaymentRequest request, CancellationToken ct)
    {
        if (request.Amount <= 0) return BadRequest(new { message = "Valor do pagamento deve ser positivo." });
        var card = await db.CreditCards.SingleOrDefaultAsync(x => x.Id == id && x.UserId == GetUserId(), ct);
        if (card is null) return NotFound();
        var invoices = await BuildInvoices(card, ct);
        var invoice = invoices.FirstOrDefault(x => x.Id == invoiceId);
        if (invoice is null) return NotFound(new { message = "Fatura não encontrada." });

        var entity = await db.CreditCardInvoices.SingleOrDefaultAsync(x => x.Id == invoiceId && x.UserId == GetUserId(), ct);
        if (entity is null)
        {
            entity = new CreditCardInvoice(GetUserId(), card.Id, invoice.ReferenceMonth, invoice.ClosingDate, invoice.DueDate);
            db.CreditCardInvoices.Add(entity);
        }
        entity.Recalculate(invoice.TotalAmount, DateTime.UtcNow);
        entity.Pay(request.Amount, DateTime.UtcNow);
        await db.SaveChangesAsync(ct);
        return Ok(new { entity.Id, entity.TotalAmount, entity.PaidAmount, status = entity.Status.ToString(), entity.PaidAt });
    }

    private async Task<List<InvoiceView>> BuildInvoices(CreditCard card, CancellationToken ct)
    {
        var purchases = await db.CreditCardPurchases.AsNoTracking().Where(x => x.CreditCardId == card.Id && x.UserId == GetUserId()).ToListAsync(ct);
        var persisted = await db.CreditCardInvoices.AsNoTracking().Where(x => x.CreditCardId == card.Id && x.UserId == GetUserId()).ToDictionaryAsync(x => x.ReferenceMonth, ct);
        var months = purchases.SelectMany(p => Enumerable.Range(1, p.Installments).Select(i => p.GetInvoiceMonth(i))).Distinct().OrderBy(x => x).ToList();
        var now = DateTime.UtcNow;
        var views = new List<InvoiceView>();
        foreach (var month in months)
        {
            var closing = DateForDay(month, card.ClosingDay);
            var due = DueDate(month, card.ClosingDay, card.DueDay);
            var lines = purchases.Where(p => Enumerable.Range(1, p.Installments).Any(i => p.GetInvoiceMonth(i) == month)).Select(p => new InvoiceLine(
                p.Id, p.Description, p.Installments, Enumerable.Range(1, p.Installments).First(i => p.GetInvoiceMonth(i) == month), p.InstallmentAmount, p.CategoryId)).ToList();
            var total = lines.Sum(x => x.Amount);
            var entity = persisted.GetValueOrDefault(month);
            var paid = entity?.PaidAmount ?? 0m;
            var status = entity?.Status ?? (now >= closing ? CreditCardInvoiceStatus.Closed : CreditCardInvoiceStatus.Open);
            if (now > due && paid < total) status = CreditCardInvoiceStatus.Overdue;
            if (paid >= total && total > 0) status = CreditCardInvoiceStatus.Paid;
            views.Add(new InvoiceView(entity?.Id ?? DeterministicGuid(card.Id, month), month, closing, due, total, paid, status.ToString(), lines));
        }
        return views.OrderByDescending(x => x.ReferenceMonth).ToList();
    }

    private static bool IsCurrentInvoice(InvoiceView x) => x.ReferenceMonth == Month(DateTime.UtcNow);
    private static DateTime Month(DateTime d) => new(d.Year, d.Month, 1, 0, 0, 0, DateTimeKind.Utc);
    private static bool ValidDay(int day) => day is >= 1 and <= 28;
    private static DateTime DateForDay(DateTime month, int day) => new(month.Year, month.Month, day, 23, 59, 59, DateTimeKind.Utc);
    private static DateTime DueDate(DateTime month, int closingDay, int dueDay) => dueDay > closingDay ? DateForDay(month, dueDay) : DateForDay(month.AddMonths(1), dueDay);
    private static DateTime CalculateInvoiceMonth(CreditCard card, DateTime purchaseDate)
    {
        var month = Month(purchaseDate);
        return purchaseDate.Day > card.ClosingDay ? month.AddMonths(1) : month;
    }
    private static Guid DeterministicGuid(Guid cardId, DateTime month)
    {
        var bytes = cardId.ToByteArray();
        var ticks = BitConverter.GetBytes(month.Ticks);
        for (var i = 0; i < Math.Min(bytes.Length, ticks.Length); i++) bytes[i] ^= ticks[i];
        return new Guid(bytes);
    }
    private Guid GetUserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var id) ? id : Guid.Empty;

    public sealed record CardRequest(string Institution, string Name, decimal CreditLimit, int ClosingDay, int DueDay, string? LastFourDigits, bool Active = true);
    public sealed record PurchaseRequest(Guid? CategoryId, string Description, decimal Amount, int Installments, DateTime PurchaseDate, DateTime? FirstInvoiceMonth, string? Notes);
    public sealed record PaymentRequest(decimal Amount);
    public sealed record InvoiceLine(Guid PurchaseId, string Description, int Installments, int InstallmentNumber, decimal Amount, Guid? CategoryId);
    public sealed record InvoiceView(Guid Id, DateTime ReferenceMonth, DateTime ClosingDate, DateTime DueDate, decimal TotalAmount, decimal PaidAmount, string Status, List<InvoiceLine> Lines);
}
