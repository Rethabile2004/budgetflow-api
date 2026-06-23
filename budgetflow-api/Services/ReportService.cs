using BudgetFlow.API.Data;
using BudgetFlow.API.DTOs.Report;
using BudgetFlow.API.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BudgetFlow.API.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _contextAccessor;

        public ReportService(AppDbContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }

        private string GetUserId() => _contextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
                                    ?? throw new UnauthorizedException("User not authenticated");

        public async Task<List<MonthlyReportResponseDto>> GetAllAsync()
        {
            var userId = GetUserId();

            var reports = await _context.MonthlyReports
                .Where(m => m.UserId == userId)
                // Sort chronologically descending so the user sees their newest timeline first
                .OrderByDescending(m => m.Year)
                .ThenByDescending(m => m.Month)
                .Select(m => new MonthlyReportResponseDto
                {
                    Month = m.Month,
                    Balance = m.Balance,
                    GeneratedAt = m.GeneratedAt,
                    Id = m.Id,
                    ReportJson = m.ReportJson,
                    TotalExpenses = m.TotalExpenses,
                    TotalIncome = m.TotalIncome,
                    Year = m.Year
                })
                .ToListAsync();

            return reports;
        }

        public async Task<MonthlyReportResponseDto> GetByIdAsync(int id)
        {
            var userId = GetUserId();
 
            // prevents users from accessing reports belonging to other accounts.
            var report = await _context.MonthlyReports.Where(m => m.Id == id && m.UserId == userId)
                .Select(m => new MonthlyReportResponseDto
                {
                    Month = m.Month,
                    Balance = m.Balance,
                    GeneratedAt = m.GeneratedAt,
                    Id = m.Id,
                    ReportJson = m.ReportJson,
                    TotalExpenses = m.TotalExpenses,
                    TotalIncome = m.TotalIncome,
                    Year = m.Year
                })
                .FirstOrDefaultAsync() ?? throw new NotFoundException("Monthly report not found.");

            return report;
        }
    }
}