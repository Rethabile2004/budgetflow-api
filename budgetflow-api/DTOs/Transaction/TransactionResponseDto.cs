using BudgetFlow.API.Models;

namespace BudgetFlow.API.DTOs.Transaction
{
    public class TransactionResponseDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public CategoryType CategoryType { get; set; }
    }
}