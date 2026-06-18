using System.ComponentModel.DataAnnotations;

namespace BudgetFlow.API.DTOs.Budget
{
    public class UpdateBudgetDto
    {
        [Required, Range(0.01, double.MaxValue)]
        public decimal LimitAmount { get; set; }
    }
}