using System.ComponentModel.DataAnnotations;

namespace BudgetFlow.API.DTOs.Transaction
{
    public class CreateTransactionDto
    {
        [Required]
        public string Description { get; set; } = string.Empty;

        [Required, Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}