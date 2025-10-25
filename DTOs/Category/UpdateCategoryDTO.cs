using System.ComponentModel.DataAnnotations;

namespace ExpenseVista.API.DTOs.Category
{
    public class UpdateCategoryDTO
    {
        public int Id { get; set; } // if to use a different type for id
        [Required]
        public string CategoryName { get; set; } = string.Empty;
    }
}
