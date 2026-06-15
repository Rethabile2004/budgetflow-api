using BudgetFlow.API.Models;

namespace BudgetFlow.API.DTOs.Category
{
    public class CategoryResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public CategoryType Type { get; set; }
    }
}