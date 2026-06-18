using BudgetFlow.API.Data;
using BudgetFlow.API.DTOs.Budget;
using BudgetFlow.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

namespace BudgetFlow.API.Services
{
    public class BudgetService:IBudgetService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContext;
        public BudgetService(AppDbContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContext = httpContext;
        }

        private string GetUserId() => _httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ??throw new Exception("User not authenticated");
        // calculates how much has been spent against a budget
        private async Task<decimal> GetSpentAmount(string userId, int categoryId, int month, int year)
        {
            return await _context.Transactions.Where(t =>
                                    t.UserId == userId && t.CategoryId == categoryId
                                    && t.Date.Month == month && t.Date.Year == year
                                    ).SumAsync(t => t.Amount);
        }
        public async Task<BudgetResponseDto> CreateAsync(CreateBudgetDto dto)
        {
            var userId = GetUserId();
            var category = await _context.Categories.FirstOrDefaultAsync(c =>
                                        c.UserId == userId
                                        && c.Id==dto.CategoryId
                                        ) ?? throw new Exception("Category not found.");
            var exists = await _context.Budgets.AnyAsync(b => 
                                        b.UserId == userId
                                        &&b.Month==dto.Month
                                        &&b.CategoryId==dto.CategoryId
                                        &&b.Year==dto.Year);
            if (exists)
                throw new Exception("Budget for this category and months already exists.");

            var budget = new Budget
            {
                UserId=userId,
                CategoryId= dto.CategoryId,
                LimitAmount=dto.LimitAmount,
                Month=dto.Month,
                Year=dto.Year,                
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
                Remaining = budget.LimitAmount - spent,
                SpentAmount = spent
            };
        }

        public async Task DeleteAsync(int id)
        {
            var userId = GetUserId();
            var budget= await _context.Budgets.FirstOrDefaultAsync(b=>
                              b.Id==id&&b.UserId==userId)
                                ?? throw new Exception("Budget not found.");
            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
        }

        public async Task<List<BudgetResponseDto>> GetAllAsync(int month, int year)
        {
            var userId = GetUserId();

            var budgets=await _context.Budgets.Include(b=>b.Category).
                        Where(b=>b.UserId==userId&&b.Month==month&& b.Year==year).ToListAsync();

            var result = new List<BudgetResponseDto>();
            foreach(var budget in budgets)
            {
                var spent = await GetSpentAmount(userId, budget.CategoryId, month, year);
                result.Add(new BudgetResponseDto
                {
                    Id=budget.Id,
                    CategoryId=budget.CategoryId,
                    CategoryName=budget.Category.Name,
                    LimitAmount=budget.LimitAmount,
                    Month=budget.Month,
                    Year=budget.Year,
                    Remaining=budget.LimitAmount-spent,
                    SpentAmount=spent
                });
            }
            return result;
        }

        public async Task<BudgetResponseDto> GetByIdAsync(int id)
        {
            var userId = GetUserId();

            var budget = await _context.Budgets.Include(b => b.Category).
                            FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId)
                            ?? throw new Exception("Budget not found.");
            var spent = await GetSpentAmount(userId,budget.CategoryId,budget.Month,budget.Year);

            return new BudgetResponseDto
            {
                Id = budget.Id,
                CategoryId = budget.CategoryId,
                CategoryName = budget.Category.Name,
                LimitAmount = budget.LimitAmount,
                Month = budget.Month,
                Year = budget.Year,
                Remaining = budget.LimitAmount - spent,
                SpentAmount = spent
            };
        }

        public async Task<BudgetResponseDto> UpdateAsync(int id, UpdateBudgetDto dto)
        {
            var userId = GetUserId();
            var budget = await _context.Budgets.Include(b => b.Category)
                            .FirstOrDefaultAsync(b => b.Id == id&&b.UserId==userId) ??
                                throw new Exception("Budget not found.");
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
                Remaining = budget.LimitAmount - spent,
                SpentAmount = spent
            };
        }
    }
}
