using System.ComponentModel.DataAnnotations;

namespace BudgetFlow.API.DTOs.Summary
{
    public class SummaryQueryDto
    {
        [Required]
        public DateTime From { get; set; }

        [Required]
        public DateTime To { get; set; }
    }
}