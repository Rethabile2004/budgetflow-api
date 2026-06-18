using BudgetFlow.API.DTOs.Budget;

namespace BudgetFlow.API.Services
{
    public interface IBudgetService
    {
        Task<List<BudgetResponseDto>> GetAllAsync(int month, int year);
        Task<BudgetResponseDto> GetByIdAsync(int id);
        Task<BudgetResponseDto> CreateAsync(CreateBudgetDto dto);
        Task<BudgetResponseDto> UpdateAsync(int id, UpdateBudgetDto dto);
        Task DeleteAsync(int id);
    }
}