namespace BudgetFlow.API.Services
{
    public interface IMonthlyReportJob
    {
        Task GenerateReportsAsync();
    }
}
