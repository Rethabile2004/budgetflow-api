
using BudgetFlow.API.Data;
using BudgetFlow.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BudgetFlow.API.Services
{
    public class MonthlyReportJob : IMonthlyReportJob
    {
        private readonly AppDbContext _context;
        public MonthlyReportJob(AppDbContext context)
        {
            _context = context;
        }
        public async Task GenerateReportsAsync()
        {
            var today = DateTime.UtcNow;
            var targetDate = today.AddMonths(-1);
            var month = targetDate.Month;
            var year = targetDate.Year;

            var userIds = await _context.Users.Select(u => u.Id).ToListAsync();

            foreach(var id in userIds)
            {
                await GenerateReportForUser(id, month, year);
            }
        }
        private async Task GenerateReportForUser(string userId,int month, int year)
        {
            var exists = await _context.MonthlyReports.AnyAsync(r => r.UserId == userId &&
                                        r.Month == month && r.Year == year);
            if (exists) return;

            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId && t.Date.Month == month && t.Date.Year == year) // filter to target month only
                .ToListAsync();

            var totalIncome = transactions.Where(t => t.Category.Type == CategoryType.Income).Sum(t => t.Amount);
            var totalExpense = transactions.Where(t => t.Category.Type == CategoryType.Expense).Sum(t => t.Amount);

            var breakdown = transactions.GroupBy(
                            t => new { t.CategoryId, t.Category.Name, t.Category.Type }).
                            Select(g =>
                                new
                                {
                                    CategoryId = g.Key.CategoryId,
                                    CategoryName = g.Key.Name,
                                    Type = g.Key.Type.ToString(),
                                    Total = g.Sum(t => t.Amount)
                                }).ToList();

            var report = new MonthlyReport
            {
                Balance= totalIncome-totalExpense,
                GeneratedAt=DateTime.UtcNow,
                Month=month,
                ReportJson=JsonSerializer.Serialize(breakdown),
                TotalExpenses=totalExpense,
                TotalIncome=totalIncome,
                UserId=userId,
                Year=year
            };

            _context.MonthlyReports.Add(report);
            await _context.SaveChangesAsync();
        }
    }
}
