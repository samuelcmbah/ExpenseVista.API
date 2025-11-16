using System.ComponentModel.DataAnnotations;

namespace ExpenseVista.API.DTOs.Budget
{
    public class BudgetCreateDTO
    {
        [Required]
        [Range(0.0, double.MaxValue, ErrorMessage = "Monthly limit must be zero or positive.")]
        public decimal MonthlyLimit { get; set; }

        // The client provides the month/year for the budget
        [Required]
        public DateTime BudgetMonth { get; set; }
    }
}