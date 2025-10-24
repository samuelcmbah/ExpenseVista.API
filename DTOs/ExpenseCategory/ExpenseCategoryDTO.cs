namespace ExpenseVista.API.DTOs.ExpenseCategory
{
    public class ExpenseCategoryDTO
    {
        public int Id { get; set; } // if to use a different type for id
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
