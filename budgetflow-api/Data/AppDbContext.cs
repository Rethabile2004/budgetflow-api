using BudgetFlow.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.API.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Budget> Budgets => Set<Budget>();
        public DbSet<Transaction> Transactions=> Set<Transaction>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<MonthlyReport> MonthlyReports=> Set<MonthlyReport>();
        public DbSet<RefreshToken> RefreshTokens=> Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);

            builder.Entity<Budget>()
                .Property(b => b.LimitAmount)
                .HasPrecision(18, 2);

            builder.Entity<MonthlyReport>()
                .Property(r => r.TotalIncome).HasPrecision(18, 2);
            builder.Entity<MonthlyReport>()
                .Property(r => r.TotalExpenses).HasPrecision(18, 2);
            builder.Entity<MonthlyReport>()
                .Property(r => r.Balance).HasPrecision(18, 2);

            builder.Entity<Transaction>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Budget>()
                .HasOne(b => b.Category)
                .WithMany(c => c.Budgets)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
