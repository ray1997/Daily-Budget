using DailyBudget.Api.Models;

namespace DailyBudget.Api.Dtos;

public sealed record OidcLoginRequest(string Code, string RedirectUri, string CodeVerifier);
public sealed record AuthResponse(string Token, DateTimeOffset ExpiresAt, UserDto User);
public sealed record UserDto(Guid Id, string Email, string DisplayName);

public sealed record UpsertCycleRequest(
    decimal StartingBalance,
    DateOnly StartDate,
    DateOnly? NextSalaryDate,
    SalaryCycleMode SalaryCycleMode,
    int? FixedSalaryDay,
    DateOnly? BiweeklyAnchorDate,
    DayOfWeek? LastWeekday,
    string CurrencySymbol);

public sealed record TransactionRequest(decimal Amount, string? Note, DateOnly TransactionDate, TransactionType Type);
public sealed record TransactionDto(Guid Id, decimal Amount, string Note, DateTimeOffset Timestamp, DateOnly TransactionDate, TransactionType Type);
public sealed record CycleDto(Guid Id, decimal StartingBalance, DateOnly StartDate, DateOnly NextSalaryDate, SalaryCycleMode SalaryCycleMode, int? FixedSalaryDay, DateOnly? BiweeklyAnchorDate, DayOfWeek? LastWeekday, string CurrencySymbol);
public sealed record AllowancePointDto(DateOnly Date, decimal DailyAllowance);

public sealed record DashboardDto(
    CycleDto? Cycle,
    decimal RemainingMoney,
    int DaysUntilSalary,
    decimal DailyBudget,
    decimal SpentToday,
    decimal TodayProgressPercent,
    decimal ProjectedRemaining,
    IReadOnlyList<AllowancePointDto> AllowanceHistory,
    IReadOnlyList<TransactionDto> Transactions);
