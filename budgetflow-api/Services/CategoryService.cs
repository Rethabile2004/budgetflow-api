using BudgetFlow.API.Data;
using BudgetFlow.API.DTOs.Category;
using BudgetFlow.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

        public async Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto)
        {
            var userId = GetUserId();

            var category = new Category
            {
                Name=dto.Name,
                Type=dto.Type,
                UserId=userId
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new CategoryResponseDto
            {
                Type = category.Type,
                Id = category.Id,
                Name = category.Name
            };
        }

        async Task ICategoryService.DeleteAsync(int id)
        {
            var userId = GetUserId();

            var category = await _context.Categories.FirstOrDefaultAsync(
                c => c.Id == id && c.UserId == userId) ?? throw new Exception(
                    "Category not found.");
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CategoryResponseDto>> GetAllAsync()
        {
            var userId = GetUserId();

            return await _context.Categories.Where(c => c.UserId == userId).
                Select(c => new CategoryResponseDto
                {
                    Id=c.Id,
                    Name=c.Name,
                    Type=c.Type
                }).ToListAsync();
        }

        public async Task<CategoryResponseDto> GetByIdAsync(int id)
        {
            var userId = GetUserId();

            var category = await _context.Categories.FirstOrDefaultAsync(
                c => c.Id == id && c.UserId == userId) ?? throw new Exception("Category not found.");

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

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id
            && c.UserId == userId) ?? throw new Exception("Category not found.");

            category.Name = dto.Name;
            category.Type = dto.Type;

            await _context.SaveChangesAsync();
            return new CategoryResponseDto
            {
                Type = category.Type,
                Name = category.Name,
                Id = category.Id
            };
        }
        private string GetUserId() => _httpContextAccessor.HttpContext!.
            User.FindFirstValue(ClaimTypes.NameIdentifier)??throw new 
            Exception("User not authenticated.");
    }
}
