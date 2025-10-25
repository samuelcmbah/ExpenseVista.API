using System.ComponentModel.DataAnnotations;

namespace ExpenseVista.API.DTOs.Budget
{
    public class BudgetUpdateDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Monthly limit must be positive.")]
        public decimal MonthlyLimit { get; set; }

        // The month/year for the budget (should usually not be changed, but included for completeness)
        [Required]
        public DateTime BudgetMonth { get; set; }
    }
}
