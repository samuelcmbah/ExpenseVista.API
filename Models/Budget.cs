
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseVista.API.Models
{
    public class Budget
    {
        [Key]
        public int Id { get; set; }

        // The maximum amount the user intends to spend/save within the period
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MonthlyLimit { get; set; }


        /// <summary>
        /// The month/year this budget applies to (e.g., set to the 1st day of the month)
        /// </summary>
        [Required]
        public DateTime BudgetMonth { get; set; }

        // Foreign Key to the ApplicationUser
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        // Navigation Property
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}