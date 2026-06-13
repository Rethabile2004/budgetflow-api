using BudgetFlow.API.Models;

namespace BudgetFlow.API.Models
{
    public class Budget
    {
        public int Id { get; set; }
        public decimal LimitAmount { get; set; }
        public int Month { get; set; }  
        public int Year { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;
    }
}