
using BudgetFlow.API.Data;

namespace BudgetFlow.API.Services
{
    public class MonthlyReportJob : IMonthlyReportJob
    {
        private readonly AppDbContext _context;
        public MonthlyReportJob(AppDbContext context)
        {
            _context = context;
        }
        public async Task GenerateReportsAsync()
        {

            throw new NotImplementedException();
        }
    }
}
