using FinanceFlow.Domain.Entities;
using FinanceFlow.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceFlow.Infrastructure.Data;

public sealed class FinanceFlowDbContext(DbContextOptions<FinanceFlowDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<CreditCard> CreditCards => Set<CreditCard>();
    public DbSet<CreditCardPurchase> CreditCardPurchases => Set<CreditCardPurchase>();
    public DbSet<CreditCardInvoice> CreditCardInvoices => Set<CreditCardInvoice>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuthToken> AuthTokens => Set<AuthToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(320).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
        });
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.UserId);
            entity.Property(x => x.Institution).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Balance).HasPrecision(18, 2);
            entity.Property(x => x.YieldPercentage).HasPrecision(8, 4);
        });
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.Date });
            entity.Property(x => x.Description).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Notes).HasMaxLength(500);
        });
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.Name }).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Color).HasMaxLength(20).IsRequired();
        });
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.CategoryId, x.Month }).IsUnique();
            entity.Property(x => x.LimitAmount).HasPrecision(18, 2);
        });
        modelBuilder.Entity<CreditCard>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.UserId);
            entity.Property(x => x.Institution).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastFourDigits).HasMaxLength(4);
            entity.Property(x => x.CreditLimit).HasPrecision(18, 2);
        });
        modelBuilder.Entity<CreditCardPurchase>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.CreditCardId, x.FirstInvoiceMonth });
            entity.Property(x => x.Description).HasMaxLength(180).IsRequired();
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.Notes).HasMaxLength(500);
        });
        modelBuilder.Entity<CreditCardInvoice>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.CreditCardId, x.ReferenceMonth }).IsUnique();
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.PaidAmount).HasPrecision(18, 2);
        });
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => x.UserId);
        });
        modelBuilder.Entity<AuthToken>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => new { x.UserId, x.Type });
        });
    }
}
