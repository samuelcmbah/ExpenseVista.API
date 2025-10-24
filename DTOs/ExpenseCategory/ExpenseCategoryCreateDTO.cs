namespace ExpenseVista.API.DTOs.ExpenseCategory
{
    public class ExpenseCategoryCreateDTO
    {
        public string Name { get; set; } =string.Empty;
        public string? Description { get; set; }
    }
}
