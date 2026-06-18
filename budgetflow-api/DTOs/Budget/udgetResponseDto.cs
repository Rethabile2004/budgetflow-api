namespace BudgetFlow.API.DTOs.Budget
{
    public class BudgetResponseDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal LimitAmount { get; set; }
        public decimal SpentAmount { get; set; }  // how much has been spent so far
        public decimal Remaining { get; set; }     // LimitAmount - SpentAmount
        public int Month { get; set; }
        public int Year { get; set; }
    }
}