using BudgetFlow.API.Models;

namespace BudgetFlow.API.DTOs.Transaction
{
    public class TransactionQueryDto
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int? CategoryId { get; set; }
        public CategoryType? Type { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}