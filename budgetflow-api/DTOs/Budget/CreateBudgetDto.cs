using System.ComponentModel.DataAnnotations;

namespace BudgetFlow.API.DTOs.Budget
{
    public class CreateBudgetDto
    {
        [Required]
        public int CategoryId { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal LimitAmount { get; set; }

        [Required, Range(1, 12)]
        public int Month { get; set; }

        [Required, Range(2000, 9999)]
        public int Year { get; set; }
    }
}