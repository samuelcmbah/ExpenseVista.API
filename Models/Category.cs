namespace ExpenseVista.API.Models
{
    public class Category
    {
        public int Id { get; set; } // if to use a different type for id
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryType { get; set; } = string.Empty;
    }
}
