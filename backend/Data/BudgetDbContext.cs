using DailyBudget.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DailyBudget.Api.Data;

public sealed class BudgetDbContext(DbContextOptions<BudgetDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<BudgetCycle> BudgetCycles => Set<BudgetCycle>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<HolidayOverride> HolidayOverrides => Set<HolidayOverride>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>().HasIndex(user => user.OidcSubject).IsUnique();
        modelBuilder.Entity<BudgetCycle>().Property(cycle => cycle.StartingBalance).HasPrecision(18, 2);
        modelBuilder.Entity<Transaction>().Property(transaction => transaction.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<BudgetCycle>().HasMany(cycle => cycle.Transactions).WithOne(transaction => transaction.BudgetCycle).HasForeignKey(transaction => transaction.BudgetCycleId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<AppUser>().HasMany(user => user.BudgetCycles).WithOne(cycle => cycle.User).HasForeignKey(cycle => cycle.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
