namespace ExpenseVista.API.DTOs.Analytics
{
    public class SpendingCategoryDTO
    {
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal Percentage { get; set; }
    }
}
