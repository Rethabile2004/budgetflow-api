using BudgetFlow.API.DTOs.Common;
using BudgetFlow.API.DTOs.Transaction;

namespace BudgetFlow.API.Services
{
    public interface ITransactionService
    {
        Task<PagedResult<TransactionResponseDto>> GetAllAsync(TransactionQueryDto queryDto);
        Task<TransactionResponseDto> GetByIdAsync(int id);
        Task<TransactionResponseDto> CreateAsync(CreateTransactionDto dto);
        Task<TransactionResponseDto> UpdateAsync(int id,UpdateTransactionDto dto);
        Task DeleteAsync(int id);
    }
}
