namespace DailyBudget.Api.Services;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "DailyBudget";
    public string Audience { get; set; } = "DailyBudgetFrontend";
    public string SigningKey { get; set; } = "replace-with-at-least-32-characters-for-production";
    public int SessionDays { get; set; } = 14;
}
