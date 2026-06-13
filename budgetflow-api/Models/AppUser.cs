using Microsoft.AspNetCore.Identity;

namespace BudgetFlow.API.Models
{
    public class AppUser:IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt{ get; set; } = DateTime.UtcNow;
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
