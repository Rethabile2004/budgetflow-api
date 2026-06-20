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

            // get all the users transactions
            var transactions = await _context.Transactions.Include(t => t.Category)
                                .Where(t => t.UserId == userId &&
                                            t.Date >= query.From &&
                                            t.Date <= query.To).ToListAsync();

            // separate the transaction types by income and expense
            var incomeTransactions= transactions.Where(t => t.Category.Type == CategoryType.Income).ToList();
            var expenseTransactions = transactions.Where(t => t.Category.Type == CategoryType.Expense).ToList();

            // calculate the total income and expense
            var totalIncome = incomeTransactions.Sum(t => t.Amount);
            var totalExpense = expenseTransactions.Sum(t => t.Amount);

            // Group transactions by category and sum each group
            var incomeByCategory = incomeTransactions.GroupBy(t => new { t.CategoryId, t.Category.Name })
                                        .Select(g => new CategorySummaryDto
                                        {
                                            CategoryId = g.Key.CategoryId,
                                            CategoryName = g.Key.Name,
                                            Total = g.Sum(t => t.Amount)
                                        }).ToList();
            var expenseByCategory = expenseTransactions.GroupBy(t => new { t.CategoryId, t.Category.Name })
                                    .Select(g => new CategorySummaryDto
                                    {
                                        CategoryId = g.Key.CategoryId,
                                        CategoryName = g.Key.Name,
                                        Total = g.Sum(t => t.Amount)
                                    }).ToList();

            return new SummaryResponseDto
            {
                TotalIncome=totalIncome,
                TotalExpenses=totalExpense,
                Balance=totalIncome-totalExpense,
                ExpenseByCategory=expenseByCategory,
                IncomeByCategory=incomeByCategory,
                From=query.From,
                To=query.To
            };
        }
    }
}
