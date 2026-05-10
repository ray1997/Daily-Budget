using System.Security.Claims;
using DailyBudget.Api.Data;
using DailyBudget.Api.Dtos;
using DailyBudget.Api.Models;
using DailyBudget.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyBudget.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public sealed class BudgetController(BudgetDbContext db, BudgetCalculator calculator) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> Dashboard(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        var cycle = await ActiveCycle(userId).FirstOrDefaultAsync(cancellationToken);
        var transactions = cycle is null
            ? []
            : await db.Transactions.Where(t => t.UserId == userId && t.BudgetCycleId == cycle.Id).ToListAsync(cancellationToken);
        return Ok(calculator.BuildDashboard(cycle, transactions, DateOnly.FromDateTime(DateTime.Now)));
    }

    [HttpPost("cycles")]
    public async Task<ActionResult<CycleDto>> StartCycle(UpsertCycleRequest request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        await db.BudgetCycles.Where(c => c.UserId == userId && c.IsActive).ExecuteUpdateAsync(s => s.SetProperty(c => c.IsActive, false), cancellationToken);
        var cycle = new BudgetCycle
        {
            UserId = userId,
            StartingBalance = request.StartingBalance,
            StartDate = request.StartDate,
            SalaryCycleMode = request.SalaryCycleMode,
            FixedSalaryDay = request.FixedSalaryDay,
            BiweeklyAnchorDate = request.BiweeklyAnchorDate,
            LastWeekday = request.LastWeekday,
            CurrencySymbol = string.IsNullOrWhiteSpace(request.CurrencySymbol) ? "฿" : request.CurrencySymbol,
            IsActive = true
        };
        cycle.NextSalaryDate = request.NextSalaryDate ?? calculator.CalculateNextSalaryDate(cycle, await UserHolidays(userId).ToListAsync(cancellationToken), request.StartDate);
        db.BudgetCycles.Add(cycle);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Dashboard), new CycleDto(cycle.Id, cycle.StartingBalance, cycle.StartDate, cycle.NextSalaryDate, cycle.SalaryCycleMode, cycle.FixedSalaryDay, cycle.BiweeklyAnchorDate, cycle.LastWeekday, cycle.CurrencySymbol));
    }

    [HttpPost("transactions")]
    public async Task<ActionResult<TransactionDto>> AddTransaction(TransactionRequest request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        var cycle = await ActiveCycle(userId).FirstOrDefaultAsync(cancellationToken);
        if (cycle is null) return BadRequest("Start a budget cycle before adding transactions.");
        var transaction = new Transaction
        {
            UserId = userId,
            BudgetCycleId = cycle.Id,
            Amount = Math.Abs(request.Amount),
            Note = request.Note?.Trim() ?? string.Empty,
            TransactionDate = request.TransactionDate,
            Type = request.Type,
            Timestamp = DateTimeOffset.UtcNow
        };
        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/transactions/{transaction.Id}", ToDto(transaction));
    }

    [HttpPut("transactions/{id:guid}")]
    public async Task<ActionResult<TransactionDto>> UpdateTransaction(Guid id, TransactionRequest request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        var transaction = await db.Transactions.SingleOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken);
        if (transaction is null) return NotFound();
        transaction.Amount = Math.Abs(request.Amount);
        transaction.Note = request.Note?.Trim() ?? string.Empty;
        transaction.TransactionDate = request.TransactionDate;
        transaction.Type = request.Type;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(transaction));
    }

    [HttpDelete("transactions/{id:guid}")]
    public async Task<IActionResult> DeleteTransaction(Guid id, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        var transaction = await db.Transactions.SingleOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken);
        if (transaction is null) return NotFound();
        db.Transactions.Remove(transaction);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? throw new InvalidOperationException("Missing user id."));
    private IQueryable<BudgetCycle> ActiveCycle(Guid userId) => db.BudgetCycles.Where(c => c.UserId == userId && c.IsActive).OrderByDescending(c => c.CreatedAt);
    private IQueryable<HolidayOverride> UserHolidays(Guid userId) => db.HolidayOverrides.Where(h => h.UserId == userId);
    private static TransactionDto ToDto(Transaction t) => new(t.Id, t.Amount, t.Note, t.Timestamp, t.TransactionDate, t.Type);
}
