using ExpenseVista.API.DTOs.Category;
using ExpenseVista.API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ExpenseVista.API.DTOs.Transaction
{
    public class TransactionDTO
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public TransactionType Type { get; set; }
        public DateTime TransactionDate { get; set; }
        public CategoryDTO Category { get; set; } = new();

    }
}
