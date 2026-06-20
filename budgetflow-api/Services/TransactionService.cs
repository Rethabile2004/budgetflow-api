using BudgetFlow.API.Data;
using BudgetFlow.API.DTOs.Common;
using BudgetFlow.API.DTOs.Transaction;
using BudgetFlow.API.Exceptions;
using BudgetFlow.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BudgetFlow.API.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly AppDbContext _context;
        public TransactionService(AppDbContext context, IHttpContextAccessor httpContext)
        {
            _httpContext= httpContext;
            _context = context;
        }
        async Task<TransactionResponseDto> ITransactionService.CreateAsync(CreateTransactionDto dto)
        {
            var userId = GetUserId();
            // verify if the category belongs to the user or even exists
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.UserId == userId)
                ?? throw new NotFoundException("Category not found.");

            var transaction = new Transaction
            {
                Amount = dto.Amount,
                CategoryId = dto.CategoryId,
                Description = dto.Description,
                Date = dto.Date,                
                UserId = userId,
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return new TransactionResponseDto
            {
                Amount=transaction.Amount,
                CategoryId=transaction.CategoryId,
                CategoryName=transaction.Category.Name,
                CategoryType=transaction.Category.Type,
                Date=transaction.Date,
                Description=transaction.Description,
                Id = transaction.Id               
            };
        }
        private string GetUserId() => _httpContext.HttpContext!.User.
            FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedException("User not authenticated.");
        public async Task DeleteAsync(int id)
        {
            var userId = GetUserId();

            var transaction = await _context.Transactions.FirstOrDefaultAsync(t =>
                t.Id == id && t.UserId == userId) ?? throw 
                    new NotFoundException("Transaction not found.");

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<TransactionResponseDto>> GetAllAsync(TransactionQueryDto queryDto)
        {
            var userId = GetUserId();

            // base query
            var queryable = _context.Transactions.Include(t => t.Category).
                Where(t => t.UserId == userId);

            // applying filters
            if(queryDto.From.HasValue)
                queryable=queryable.Where(t=>t.Date>=queryDto.From.Value);
            if (queryDto.To.HasValue)
                queryable = queryable.Where(t => t.Date <= queryDto.To.Value);
            if (queryDto.Type.HasValue)
                queryable = queryable.Where(t => t.Category.Type == queryDto.Type.Value);
            if (queryDto.CategoryId.HasValue)
                queryable = queryable.Where(t => t.CategoryId == queryDto.CategoryId.Value);

            // count total
            var totalCount = await queryable.CountAsync();

            // apply pagination
            var items = await queryable.OrderByDescending(t => t.Date).
                Skip((queryDto.Page - 1) * queryDto.PageSize).Take(queryDto.PageSize).
                Select(t => new TransactionResponseDto
                {
                    Amount = t.Amount,
                    CategoryId = t.CategoryId,
                    CategoryName = t.Category.Name,
                    CategoryType = t.Category.Type,
                    Date = t.Date,
                    Description = t.Description,
                    Id = t.Id
                }).ToListAsync();

            return new PagedResult<TransactionResponseDto>
            {
                Items=items,
                Page=queryDto.Page,
                PageSize=queryDto.PageSize,
                TotalCount=totalCount
            };
        }

        public async Task<TransactionResponseDto> GetByIdAsync(int id)
        {
            var userId = GetUserId();

            var transaction = await _context.Transactions.Include(t => t.Category).
                FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId)
                ?? throw new NotFoundException("Transaction not found.");

            return new TransactionResponseDto
            {
                Id = transaction.Id,
                Description = transaction.Description,
                Amount = transaction.Amount,
                Date = transaction.Date,
                CategoryId = transaction.CategoryId,
                CategoryName = transaction.Category.Name,
                CategoryType = transaction.Category.Type
            };
        }

        public async Task<TransactionResponseDto> UpdateAsync(int id, UpdateTransactionDto dto)
        {
            var userId = GetUserId();

            var transaction = await _context.Transactions.FirstOrDefaultAsync(
                        t => t.Id == id && t.UserId == userId) ?? throw new NotFoundException("Transaction not found.");
            // verify if the category belongs to this user
            var category= await _context.Categories.FirstOrDefaultAsync(
                    t=>t.Id==dto.CategoryId && t.UserId == userId)
                ?? throw new BadRequestException("Invalid category id.");

            transaction.Description = dto.Description;
            transaction.Amount = dto.Amount;
            transaction.Date = dto.Date;
            transaction.CategoryId = dto.CategoryId;

            await _context.SaveChangesAsync();

            return new TransactionResponseDto
            {
                Id = transaction.Id,
                Description = transaction.Description,
                Amount = transaction.Amount,
                Date = transaction.Date,
                CategoryId = transaction.CategoryId,
                CategoryName = transaction.Category.Name,
                CategoryType = transaction.Category.Type
            };
        }
    }
}
