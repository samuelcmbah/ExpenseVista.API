namespace ExpenseVista.API.Models
{
    public class ExpenseCategory
    {
        public int Id { get; set; } // if to use a different type for id
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
