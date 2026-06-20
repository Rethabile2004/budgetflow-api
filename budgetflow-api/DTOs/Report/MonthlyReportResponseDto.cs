namespace BudgetFlow.API.DTOs.Report
{
    public class MonthlyReportResponseDto
    {
        public int Id { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal Balance { get; set; }
        public string ReportJson { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
    }
}