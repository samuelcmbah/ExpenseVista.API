namespace ExpenseVista.API.DTOs.ExpenseCategory
{
    public class CreateCategoryDTO
    {
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryType { get; set; } = string.Empty;
        //public ICollection<Transaction> Transactions { get; set; }
    }
}
