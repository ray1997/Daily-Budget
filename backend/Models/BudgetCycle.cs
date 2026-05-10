using System.ComponentModel.DataAnnotations;

namespace DailyBudget.Api.Models;

public sealed class BudgetCycle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public decimal StartingBalance { get; set; }
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateOnly NextSalaryDate { get; set; }
    public SalaryCycleMode SalaryCycleMode { get; set; } = SalaryCycleMode.FixedDate;
    public int? FixedSalaryDay { get; set; }
    public DateOnly? BiweeklyAnchorDate { get; set; }
    public DayOfWeek? LastWeekday { get; set; }
    [MaxLength(8)] public string CurrencySymbol { get; set; } = "฿";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<Transaction> Transactions { get; set; } = [];
}
