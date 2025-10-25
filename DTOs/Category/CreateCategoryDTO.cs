using System.ComponentModel.DataAnnotations;

namespace ExpenseVista.API.DTOs.Category
{
    public class CreateCategoryDTO
    {
        [Required]
        public string CategoryName { get; set; } = string.Empty;
        
    }
}
