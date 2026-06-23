using BudgetFlow.API.DTOs.Transaction;
using BudgetFlow.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BudgetFlow.API.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    [Authorize]
    [EnableRateLimiting("read")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TransactionQueryDto query)
        {
            var result = await _transactionService.GetAllAsync(query);
            return Ok(result);
        }

        [HttpGet("{id}", Name = "GetTransactionById")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _transactionService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        [EnableRateLimiting("write")]
        public async Task<IActionResult> Create(CreateTransactionDto dto)
        {
            var result = await _transactionService.CreateAsync(dto);

            return CreatedAtRoute("GetTransactionById", new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [EnableRateLimiting("write")]
        public async Task<IActionResult> Update(int id, UpdateTransactionDto dto)
        {
            var result = await _transactionService.UpdateAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [EnableRateLimiting("write")]
        public async Task<IActionResult> Delete(int id)
        {
            await _transactionService.DeleteAsync(id);
            return NoContent();
        }
    }
}