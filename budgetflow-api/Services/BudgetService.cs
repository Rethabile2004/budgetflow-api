using BudgetFlow.API.Data;
using BudgetFlow.API.DTOs.Budget;
using BudgetFlow.API.Exceptions;
using BudgetFlow.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BudgetFlow.API.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContext;

        public BudgetService(AppDbContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContext = httpContext;
        }

        // Pulls the logged-in user's ID from their JWT claims.
        private string GetUserId() =>
            _httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedException("User not authenticated");

        // Adds up all transactions for a specific category in a given month and year.
        private async Task<decimal> GetSpentAmount(string userId, int categoryId, int month, int year)
        {
            return await _context.Transactions
                .Where(t =>
                    t.UserId == userId &&
                    t.CategoryId == categoryId &&
                    t.Date.Month == month &&
                    t.Date.Year == year)
                .SumAsync(t => t.Amount);
        }

        public async Task<BudgetResponseDto> CreateAsync(CreateBudgetDto dto)
        {
            var userId = GetUserId();

            // Make sure the category belongs to this user before creating a budget for it.
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Id == dto.CategoryId)
                ?? throw new NotFoundException("Category not found.");

            // One budget per category per month, no duplicates.
            var exists = await _context.Budgets.AnyAsync(b =>
                b.UserId == userId &&
                b.Month == dto.Month &&
                b.CategoryId == dto.CategoryId &&
                b.Year == dto.Year);

            if (exists)
                throw new BadRequestException("Budget for this category and month already exists."); // fixed typo: "months" -> "month"

            var budget = new Budget
            {
                UserId = userId,
                CategoryId = dto.CategoryId,
                LimitAmount = dto.LimitAmount,
                Month = dto.Month,
                Year = dto.Year,
            };

            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();

            var spent = await GetSpentAmount(userId, dto.CategoryId, dto.Month, dto.Year);

            return new BudgetResponseDto
            {
                Id = budget.Id,
                CategoryId = budget.CategoryId,
                CategoryName = category.Name,
                LimitAmount = budget.LimitAmount,
                Month = budget.Month,
                Year = budget.Year,
                SpentAmount = spent,
                Remaining = budget.LimitAmount - spent,
            };
        }

        public async Task DeleteAsync(int id)
        {
            var userId = GetUserId();

            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId)
                ?? throw new NotFoundException("Budget not found.");

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
        }

        public async Task<List<BudgetResponseDto>> GetAllAsync(int month, int year)
        {
            var userId = GetUserId();

            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId && b.Month == month && b.Year == year)
                .ToListAsync();

            // Fetch all transaction sums for this month in one query instead of one per budget.
            var categoryIds = budgets.Select(b => b.CategoryId).ToList();
            var spentLookup = await _context.Transactions
                .Where(t =>
                    t.UserId == userId &&
                    categoryIds.Contains(t.CategoryId) &&
                    t.Date.Month == month &&
                    t.Date.Year == year)
                .GroupBy(t => t.CategoryId)
                .Select(g => new { CategoryId = g.Key, Total = g.Sum(t => t.Amount) })
                .ToDictionaryAsync(x => x.CategoryId, x => x.Total);

            return budgets.Select(budget =>
            {
                // If no transactions exist for this category yet, default to 0.
                var spent = spentLookup.GetValueOrDefault(budget.CategoryId, 0);
                return new BudgetResponseDto
                {
                    Id = budget.Id,
                    CategoryId = budget.CategoryId,
                    CategoryName = budget.Category.Name,
                    LimitAmount = budget.LimitAmount,
                    Month = budget.Month,
                    Year = budget.Year,
                    SpentAmount = spent,
                    Remaining = budget.LimitAmount - spent,
                };
            }).ToList();
        }

        public async Task<BudgetResponseDto> GetByIdAsync(int id)
        {
            var userId = GetUserId();

            var budget = await _context.Budgets
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId)
                ?? throw new NotFoundException("Budget not found.");

            var spent = await GetSpentAmount(userId, budget.CategoryId, budget.Month, budget.Year);

            return new BudgetResponseDto
            {
                Id = budget.Id,
                CategoryId = budget.CategoryId,
                CategoryName = budget.Category.Name,
                LimitAmount = budget.LimitAmount,
                Month = budget.Month,
                Year = budget.Year,
                SpentAmount = spent,
                Remaining = budget.LimitAmount - spent,
            };
        }

        public async Task<BudgetResponseDto> UpdateAsync(int id, UpdateBudgetDto dto)
        {
            var userId = GetUserId();

            var budget = await _context.Budgets
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId)
                ?? throw new NotFoundException("Budget not found.");

            // Only the spending limit can be changed after a budget is created.
            budget.LimitAmount = dto.LimitAmount;
            await _context.SaveChangesAsync();

            var spent = await GetSpentAmount(userId, budget.CategoryId, budget.Month, budget.Year);

            return new BudgetResponseDto
            {
                Id = budget.Id,
                CategoryId = budget.CategoryId,
                CategoryName = budget.Category.Name,
                LimitAmount = budget.LimitAmount,
                Month = budget.Month,
                Year = budget.Year,
                SpentAmount = spent,
                Remaining = budget.LimitAmount - spent,
            };
        }
    }
}