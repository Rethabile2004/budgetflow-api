using System.ComponentModel.DataAnnotations;
using BudgetFlow.API.Models;

namespace BudgetFlow.API.DTOs.Category
{
    public class UpdateCategoryDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public CategoryType Type { get; set; }
    }
}