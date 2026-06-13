using BudgetFlow.API.Models;

namespace BudgetFlow.API.Models
{
    public class MonthlyReport
    {
        public int Id { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal Balance { get; set; }
        public string ReportJson { get; set; } = string.Empty; 
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;
    }
}