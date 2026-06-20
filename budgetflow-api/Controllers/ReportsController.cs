using BudgetFlow.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetFlow.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _service;
        public ReportsController(IReportService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> GetALl()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult>GetById(int id)
        {
                var result = await _service.GetByIdAsync(id);
                return Ok(result);
        }
    }
}
