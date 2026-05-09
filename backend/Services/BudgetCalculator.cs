using DailyBudget.Api.Dtos;
using DailyBudget.Api.Models;

namespace DailyBudget.Api.Services;

public sealed class BudgetCalculator
{
    public DashboardDto BuildDashboard(BudgetCycle? cycle, IReadOnlyList<Transaction> transactions, DateOnly today)
    {
        if (cycle is null)
        {
            return new DashboardDto(null, 0, 0, 0, 0, 0, 0, [], []);
        }

        var ordered = transactions.OrderBy(t => t.TransactionDate).ThenBy(t => t.Timestamp).ToList();
        var remaining = cycle.StartingBalance + ordered.Sum(SignedAmount);
        var daysUntilSalary = Math.Max(1, cycle.NextSalaryDate.DayNumber - today.DayNumber);
        var dailyBudget = Math.Round(remaining / daysUntilSalary, 2, MidpointRounding.AwayFromZero);
        var spentToday = ordered.Where(t => t.TransactionDate == today && t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var progress = dailyBudget <= 0 ? 100 : Math.Round(Math.Min(200, spentToday / dailyBudget * 100), 0);
        var history = BuildHistory(cycle, ordered, today);
        var projection = Math.Round(remaining - dailyBudget * daysUntilSalary, 2, MidpointRounding.AwayFromZero);

        return new DashboardDto(
            ToCycleDto(cycle),
            Math.Round(remaining, 2, MidpointRounding.AwayFromZero),
            daysUntilSalary,
            dailyBudget,
            Math.Round(spentToday, 2, MidpointRounding.AwayFromZero),
            progress,
            projection,
            history,
            ordered.OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.Timestamp).Select(ToTransactionDto).ToList());
    }

    public DateOnly CalculateNextSalaryDate(BudgetCycle cycle, IReadOnlyCollection<HolidayOverride> holidays, DateOnly fromDate)
    {
        DateOnly candidate = cycle.SalaryCycleMode switch
        {
            SalaryCycleMode.FixedDate => NextFixedDate(fromDate, cycle.FixedSalaryDay ?? cycle.NextSalaryDate.Day),
            SalaryCycleMode.Biweekly => NextBiweekly(fromDate, cycle.BiweeklyAnchorDate ?? cycle.NextSalaryDate),
            SalaryCycleMode.LastWeekdayOfMonth => LastWeekdayOfMonth(fromDate, cycle.LastWeekday ?? DayOfWeek.Friday),
            _ => cycle.NextSalaryDate
        };

        var holidayDates = holidays.Select(h => h.Date).ToHashSet();
        while (candidate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday || holidayDates.Contains(candidate))
        {
            candidate = candidate.AddDays(-1);
        }

        if (candidate <= fromDate)
        {
            return CalculateNextSalaryDate(cycle, holidays, fromDate.AddDays(1));
        }

        return candidate;
    }

    private static DateOnly NextFixedDate(DateOnly fromDate, int day)
    {
        var capped = Math.Min(day, DateTime.DaysInMonth(fromDate.Year, fromDate.Month));
        var candidate = new DateOnly(fromDate.Year, fromDate.Month, capped);
        if (candidate <= fromDate)
        {
            var next = fromDate.AddMonths(1);
            candidate = new DateOnly(next.Year, next.Month, Math.Min(day, DateTime.DaysInMonth(next.Year, next.Month)));
        }
        return candidate;
    }

    private static DateOnly NextBiweekly(DateOnly fromDate, DateOnly anchor)
    {
        var candidate = anchor;
        while (candidate <= fromDate)
        {
            candidate = candidate.AddDays(14);
        }
        return candidate;
    }

    private static DateOnly LastWeekdayOfMonth(DateOnly fromDate, DayOfWeek weekday)
    {
        var candidateMonth = fromDate;
        while (true)
        {
            var date = new DateOnly(candidateMonth.Year, candidateMonth.Month, DateTime.DaysInMonth(candidateMonth.Year, candidateMonth.Month));
            while (date.DayOfWeek != weekday)
            {
                date = date.AddDays(-1);
            }
            if (date > fromDate) return date;
            candidateMonth = candidateMonth.AddMonths(1);
        }
    }

    private static List<AllowancePointDto> BuildHistory(BudgetCycle cycle, IReadOnlyList<Transaction> transactions, DateOnly today)
    {
        var points = new List<AllowancePointDto>();
        var start = cycle.StartDate > today.AddDays(-30) ? cycle.StartDate : today.AddDays(-30);
        for (var date = start; date <= today; date = date.AddDays(1))
        {
            var remaining = cycle.StartingBalance + transactions.Where(t => t.TransactionDate <= date).Sum(SignedAmount);
            var daysLeft = Math.Max(1, cycle.NextSalaryDate.DayNumber - date.DayNumber);
            points.Add(new AllowancePointDto(date, Math.Round(remaining / daysLeft, 2, MidpointRounding.AwayFromZero)));
        }
        return points;
    }

    private static decimal SignedAmount(Transaction transaction) => transaction.Type == TransactionType.Income ? transaction.Amount : -transaction.Amount;
    private static CycleDto ToCycleDto(BudgetCycle cycle) => new(cycle.Id, cycle.StartingBalance, cycle.StartDate, cycle.NextSalaryDate, cycle.SalaryCycleMode, cycle.FixedSalaryDay, cycle.BiweeklyAnchorDate, cycle.LastWeekday, cycle.CurrencySymbol);
    private static TransactionDto ToTransactionDto(Transaction transaction) => new(transaction.Id, transaction.Amount, transaction.Note, transaction.Timestamp, transaction.TransactionDate, transaction.Type);
}
