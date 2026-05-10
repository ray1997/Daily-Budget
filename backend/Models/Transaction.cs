using System.ComponentModel.DataAnnotations;

namespace DailyBudget.Api.Models;

public sealed class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public Guid BudgetCycleId { get; set; }
    public BudgetCycle? BudgetCycle { get; set; }
    public decimal Amount { get; set; }
    [MaxLength(240)] public string Note { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public DateOnly TransactionDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public TransactionType Type { get; set; } = TransactionType.Expense;
}
