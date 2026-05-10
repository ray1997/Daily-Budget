using System.ComponentModel.DataAnnotations;

namespace DailyBudget.Api.Models;

public sealed class HolidayOverride
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public DateOnly Date { get; set; }
    [MaxLength(120)] public string Name { get; set; } = string.Empty;
}
