using BudgetFlow.API.Models;

namespace BudgetFlow.API.Models
{
    public enum CategoryType { Income, Expense }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public CategoryType Type { get; set; }

        // Foreign key — every category belongs to a user
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    }
}