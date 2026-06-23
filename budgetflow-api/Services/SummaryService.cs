using BudgetFlow.API.Data;
using BudgetFlow.API.DTOs.Summary;
using BudgetFlow.API.Exceptions;
using BudgetFlow.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BudgetFlow.API.Services
{
    public class SummaryService : ISummaryService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContext;

        public SummaryService(AppDbContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContext = httpContext;
        }

        private string GetUserId() => _httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UnauthorizedException("User not authenticated.");

        public async Task<SummaryResponseDto> GetSummaryAsync(SummaryQueryDto query)
        {
            var userId = GetUserId();

            // This builds an IQueryable so SQL Server calculates the groupings.
            var filteredTransactions = _context.Transactions
                .Where(t => t.UserId == userId && t.Date >= query.From && t.Date <= query.To);

            // Execute grouping and aggregation.
            var categorySummaries = await filteredTransactions
                .GroupBy(t => new { t.CategoryId, t.Category.Name, t.Category.Type })
                .Select(g => new
                {
                    g.Key.CategoryId,
                    g.Key.Name,
                    g.Key.Type,
                    Total = g.Sum(t => t.Amount)
                })
                .ToListAsync();

            // Separate the aggregated results into income and expense buckets in memory
            var incomeByCategory = categorySummaries
                .Where(c => c.Type == CategoryType.Income)
                .Select(c => new CategorySummaryDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.Name,
                    Total = c.Total
                }).ToList();

            var expenseByCategory = categorySummaries
                .Where(c => c.Type == CategoryType.Expense)
                .Select(c => new CategorySummaryDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.Name,
                    Total = c.Total
                }).ToList();

            // Sum up pre-calculated totals 
            var totalIncome = incomeByCategory.Sum(c => c.Total);
            var totalExpense = expenseByCategory.Sum(c => c.Total);

            return new SummaryResponseDto
            {
                TotalIncome = totalIncome,
                TotalExpenses = totalExpense,
                Balance = totalIncome - totalExpense,
                ExpenseByCategory = expenseByCategory,
                IncomeByCategory = incomeByCategory,
                From = query.From,
                To = query.To
            };
        }
    }
}