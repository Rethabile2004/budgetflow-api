namespace BudgetFlow.API.DTOs.Summary
{
    public class CategorySummaryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class SummaryResponseDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal Balance { get; set; }  // TotalIncome - TotalExpenses
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public List<CategorySummaryDto> IncomeByCategory { get; set; } = new();
        public List<CategorySummaryDto> ExpenseByCategory { get; set; } = new();
    }
}