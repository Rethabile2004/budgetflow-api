using BudgetFlow.API.DTOs.Summary;

namespace BudgetFlow.API.Services
{
    public interface ISummaryService
    {
        Task<SummaryResponseDto> GetSummaryAsync(SummaryQueryDto query);
    }
}