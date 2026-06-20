using BudgetFlow.API.DTOs.Summary;
using BudgetFlow.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SummaryController:ControllerBase
    {
        private readonly ISummaryService _summaryService;
        public SummaryController(ISummaryService summaryService)
        {
            _summaryService= summaryService;
        }
        [HttpGet]
        public async Task<IActionResult> GetSummary([FromQuery] SummaryQueryDto summaryQueryDto)
        {
            var result = await _summaryService.GetSummaryAsync(summaryQueryDto);
            return Ok(result);
        }
    }
}
