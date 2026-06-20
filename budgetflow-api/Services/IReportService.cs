using BudgetFlow.API.DTOs.Report;

namespace BudgetFlow.API.Services
{
    public interface IReportService
    {
        Task<List<MonthlyReportResponseDto>> GetAllAsync();
        Task<MonthlyReportResponseDto> GetByIdAsync(int id);
    }
}