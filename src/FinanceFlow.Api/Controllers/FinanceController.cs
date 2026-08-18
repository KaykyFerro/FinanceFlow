using System.Security.Claims;
using FinanceFlow.Domain.Entities;
using FinanceFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/finance")]
public sealed class FinanceController(FinanceFlowDbContext db) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary(int? year, int? month, CancellationToken ct)
    {
        var userId = GetUserId(); if (userId == Guid.Empty) return Unauthorized();
        var now = DateTime.UtcNow; var y = year ?? now.Year; var m = month ?? now.Month;
        if (m is < 1 or > 12) return BadRequest(new { message = "Mês inválido." });
        await EnsureDefaults(userId, ct);
        var start = new DateTime(y, m, 1, 0, 0, 0, DateTimeKind.Utc); var end = start.AddMonths(1);
        var tx = await db.Transactions.AsNoTracking().Where(x => x.UserId == userId && x.Date >= start && x.Date < end && x.Confirmed).OrderByDescending(x => x.Date).ToListAsync(ct);
        var allTx = await db.Transactions.AsNoTracking().Where(x => x.UserId == userId && x.Confirmed).ToListAsync(ct);
        var accounts = await db.Accounts.AsNoTracking().Where(x => x.UserId == userId).OrderBy(x => x.Name).ToListAsync(ct);
        var categories = await db.Categories.AsNoTracking().Where(x => x.UserId == userId).OrderBy(x => x.Name).ToListAsync(ct);
        var budgets = await db.Budgets.AsNoTracking().Where(x => x.UserId == userId && x.Month == start).ToListAsync(ct);
        var income = tx.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount);
        var expense = tx.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount);
        var investment = accounts.Where(x => x.Type == AccountType.Investment).Sum(x => x.Balance);
        var patrimonio = accounts.Sum(x => x.Balance) + allTx.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount) - allTx.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount);
        var accountRows = accounts.Select(a => new { a.Id, a.Institution, a.Name, Type = a.Type.ToString(), Balance = a.Balance + allTx.Where(t => t.AccountId == a.Id).Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount) });
        var categoryRows = categories.Select(c => new { c.Id, c.Name, c.Color, c.IsIncome });
        var txRows = tx.Select(t => new { t.Id, t.AccountId, t.CategoryId, t.Description, t.Amount, Type = t.Type.ToString(), t.Date, t.Notes, t.Confirmed });
        var budgetRows = budgets.Select(b => new { b.Id, b.CategoryId, b.Month, b.LimitAmount, Spent = tx.Where(t => t.CategoryId == b.CategoryId && t.Type == TransactionType.Expense).Sum(t => t.Amount) });
        var byDay = tx.GroupBy(t => t.Date.Day).OrderBy(g => g.Key).Select(g => new { Day = g.Key, Income = g.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount), Expense = g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount) });
        return Ok(new { year = y, month = m, income, expense, patrimonio, investment, accounts = accountRows, categories = categoryRows, transactions = txRows, budgets = budgetRows, byDay });
    }

    [HttpPost("transactions")]
    public async Task<IActionResult> CreateTransaction(TransactionRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (request.Amount <= 0 || string.IsNullOrWhiteSpace(request.Description)) return BadRequest(new { message = "Descrição e valor positivo são obrigatórios." });
        if (!Enum.TryParse<TransactionType>(request.Type, true, out var type)) return BadRequest(new { message = "Tipo de transação inválido." });
        if (!await db.Accounts.AnyAsync(x => x.Id == request.AccountId && x.UserId == userId, ct)) return BadRequest(new { message = "Conta inválida." });
        if (request.CategoryId is not null && !await db.Categories.AnyAsync(x => x.Id == request.CategoryId && x.UserId == userId, ct)) return BadRequest(new { message = "Categoria inválida." });
        var item = new Transaction(userId, request.AccountId, request.CategoryId, request.Description, request.Amount, type, request.Date.ToUniversalTime(), request.Notes);
        db.Transactions.Add(item); await db.SaveChangesAsync(ct); return Created($"/api/finance/transactions/{item.Id}", item);
    }

    [HttpPut("transactions/{id:guid}")]
    public async Task<IActionResult> UpdateTransaction(Guid id, TransactionRequest request, CancellationToken ct)
    {
        var userId = GetUserId(); var item = await db.Transactions.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);
        if (item is null) return NotFound();
        if (!Enum.TryParse<TransactionType>(request.Type, true, out var type) || request.Amount <= 0) return BadRequest(new { message = "Dados inválidos." });
        if (!await db.Accounts.AnyAsync(x => x.Id == request.AccountId && x.UserId == userId, ct)) return BadRequest(new { message = "Conta inválida." });
        item.Update(request.AccountId, request.CategoryId, request.Description, request.Amount, type, request.Date.ToUniversalTime(), request.Notes);
        await db.SaveChangesAsync(ct); return Ok(item);
    }

    [HttpDelete("transactions/{id:guid")]
    public async Task<IActionResult> DeleteTransaction(Guid id, CancellationToken ct)
    {
        var item = await db.Transactions.SingleOrDefaultAsync(x => x.Id == id && x.UserId == GetUserId(), ct);
        if (item is null) return NotFound(); db.Transactions.Remove(item); await db.SaveChangesAsync(ct); return NoContent();
    }

    [HttpPost("accounts")]
    public async Task<IActionResult> CreateAccount(AccountRequest request, CancellationToken ct)
    {
        var userId = GetUserId(); if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { message = "Nome da conta é obrigatório." });
        if (!Enum.TryParse<AccountType>(request.Type, true, out var type)) return BadRequest(new { message = "Tipo de conta inválido." });
        var item = new Account(userId, request.Institution ?? "", request.Name, type, request.Balance);
        db.Accounts.Add(item); await db.SaveChangesAsync(ct); return Created($"/api/finance/accounts/{item.Id}", item);
    }

    [HttpPut("accounts/{id:guid}")]
    public async Task<IActionResult> UpdateAccount(Guid id, AccountRequest request, CancellationToken ct)
    {
        var userId = GetUserId(); var item = await db.Accounts.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);
        if (item is null) return NotFound();
        if (!Enum.TryParse<AccountType>(request.Type, true, out var type)) return BadRequest(new { message = "Tipo de conta inválido." });
        await db.Database.ExecuteSqlRawAsync("UPDATE \"Accounts\" SET \"Institution\"={0}, \"Name\"={1}, \"Type\"={2}, \"Balance\"={3} WHERE \"Id\"={4} AND \"UserId\"={5}", request.Institution ?? "", request.Name, (int)type, request.Balance, id, userId);
        return Ok();
    }

    [HttpDelete("accounts/{id:guid}")]
    public async Task<IActionResult> DeleteAccount(Guid id, CancellationToken ct)
    {
        var item = await db.Accounts.SingleOrDefaultAsync(x => x.Id == id && x.UserId == GetUserId(), ct);
        if (item is null) return NotFound(); if (await db.Transactions.AnyAsync(x => x.AccountId == id, ct)) return Conflict(new { message = "Não é possível excluir uma conta que possui transações." });
        db.Accounts.Remove(item); await db.SaveChangesAsync(ct); return NoContent();
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory(CategoryRequest request, CancellationToken ct)
    {
        var userId = GetUserId(); if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { message = "Nome da categoria é obrigatório." });
        var item = new Category(userId, request.Name, request.Color ?? "#0aa579", request.IsIncome);
        db.Categories.Add(item); await db.SaveChangesAsync(ct); return Created($"/api/finance/categories/{item.Id}", item);
    }

    [HttpDelete("categories/{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken ct)
    {
        var item = await db.Categories.SingleOrDefaultAsync(x => x.Id == id && x.UserId == GetUserId(), ct);
        if (item is null) return NotFound(); if (await db.Transactions.AnyAsync(x => x.CategoryId == id, ct)) return Conflict(new { message = "A categoria possui transações e não pode ser excluída." });
        db.Categories.Remove(item); await db.SaveChangesAsync(ct); return NoContent();
    }

    [HttpPost("budgets")]
    public async Task<IActionResult> CreateBudget(BudgetRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (request.LimitAmount <= 0 || !await db.Categories.AnyAsync(x => x.Id == request.CategoryId && x.UserId == userId, ct)) return BadRequest(new { message = "Categoria e limite são obrigatórios." });
        var month = new DateTime(request.Month.Year, request.Month.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var existing = await db.Budgets.SingleOrDefaultAsync(x => x.UserId == userId && x.CategoryId == request.CategoryId && x.Month == month, ct);
        if (existing is not null) existing.Update(request.CategoryId, month, request.LimitAmount); else db.Budgets.Add(new Budget(userId, request.CategoryId, month, request.LimitAmount));
        await db.SaveChangesAsync(ct); return Ok();
    }

    [HttpDelete("budgets/{id:guid}")]
    public async Task<IActionResult> DeleteBudget(Guid id, CancellationToken ct)
    {
        var item = await db.Budgets.SingleOrDefaultAsync(x => x.Id == id && x.UserId == GetUserId(), ct);
        if (item is null) return NotFound(); db.Budgets.Remove(item); await db.SaveChangesAsync(ct); return NoContent();
    }

    private async Task EnsureDefaults(Guid userId, CancellationToken ct)
    {
        if (!await db.Accounts.AnyAsync(x => x.UserId == userId, ct)) db.Accounts.AddRange(
            new Account(userId, "Nubank", "Nubank", AccountType.Checking), new Account(userId, "Caixa", "Caixinha", AccountType.Savings),
            new Account(userId, "Santander", "Santander", AccountType.Checking), new Account(userId, "Carteira", "Dinheiro", AccountType.Wallet));
        if (!await db.Categories.AnyAsync(x => x.UserId == userId, ct))
        {
            var names = new[] { ("Salário", true), ("Freelance", true), ("Alimentação", false), ("Moradia", false), ("Transporte", false), ("Faculdade", false), ("Lazer", false), ("Saúde", false), ("Assinaturas", false), ("Outros", false) };
            db.Categories.AddRange(names.Select(x => new Category(userId, x.Item1, x.Item2 ? "#0aa579" : "#3569e8", x.Item2)));
        }
        await db.SaveChangesAsync(ct);
    }

    private Guid GetUserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var id) ? id : Guid.Empty;
    public sealed record TransactionRequest(Guid AccountId, Guid? CategoryId, string Description, decimal Amount, string Type, DateTime Date, string? Notes);
    public sealed record AccountRequest(string Institution, string Name, string Type, decimal Balance);
    public sealed record CategoryRequest(string Name, string? Color, bool IsIncome);
    public sealed record BudgetRequest(Guid CategoryId, DateTime Month, decimal LimitAmount);
}
