using BudgetFlow.API.DTOs.Budget;
using BudgetFlow.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetFlow.API.Controllers
{
    [ApiController]
    [Route("api/budgets")]
    [Authorize]
    public class BudgetsController : ControllerBase
    {
        private readonly IBudgetService _budgetService;

        public BudgetsController(IBudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int month, [FromQuery] int year)
        {
            var result = await _budgetService.GetAllAsync(month, year);
            return Ok(result);
        }

        [HttpGet("{id}", Name = "GetBudgetById")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _budgetService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateBudgetDto dto)
        {
            var result = await _budgetService.CreateAsync(dto);

            return CreatedAtRoute("GetBudgetById", new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateBudgetDto dto)
        {
            var result = await _budgetService.UpdateAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _budgetService.DeleteAsync(id);
            return NoContent();
        }
    }
}