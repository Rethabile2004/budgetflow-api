using System.Security.Claims;
using BudgetFlow.API.Data;
using BudgetFlow.API.DTOs.Category;
using BudgetFlow.API.Exceptions;
using BudgetFlow.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CategoryService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        private string GetUserId() =>
            _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedException("User not authenticated.");
        public async Task<List<CategoryResponseDto>> GetAllAsync()
        {
            var userId = GetUserId();
            return await _context.Categories
                .Where(c => c.UserId == userId)
                .Select(c => new CategoryResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Type = c.Type
                })
                .ToListAsync();
        }
        public async Task<CategoryResponseDto> GetByIdAsync(int id)
        {
            var userId = GetUserId();
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId)
                ?? throw new NotFoundException("Category not found.");
            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Type = category.Type
            };
        }
        public async Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto)
        {
            var userId = GetUserId();

            // Block duplicate name and type for the same user.
            var exists = await _context.Categories
                .AnyAsync(c => c.UserId == userId && c.Name == dto.Name && c.Type == dto.Type);
            if (exists)
                throw new ConflictException("A category with this name and type already exists.");

            var category = new Category
            {
                Name = dto.Name,
                Type = dto.Type,
                UserId = userId
            };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Type = category.Type
            };
        }
        public async Task<CategoryResponseDto> UpdateAsync(int id, UpdateCategoryDto dto)
        {
            var userId = GetUserId();
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId)
                ?? throw new NotFoundException("Category not found.");

            // Same duplicate check as CreateAsync, but exclude the current record so it
            // doesn't conflict with itself.
            var exists = await _context.Categories
                .AnyAsync(c => c.UserId == userId && c.Name == dto.Name && c.Type == dto.Type && c.Id != id);
            if (exists)
                throw new ConflictException("A category with this name and type already exists.");

            category.Name = dto.Name;
            category.Type = dto.Type;
            await _context.SaveChangesAsync();
            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Type = category.Type
            };
        }
        public async Task DeleteAsync(int id)
        {
            var userId = GetUserId();
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId)
                ?? throw new NotFoundException("Category not found.");
            // prevents the deleting of categories that still have transactions attatched to them
            var hasTransactions = await _context.Transactions.AnyAsync(t => t.CategoryId == id);
            if (hasTransactions)
                throw new BadRequestException("Cannot delete a category that has transactions linked to it.");
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}