using System;

namespace ExpenseVista.API.DTOs.Transaction
{
    public class TransactionLiteDTO
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public Models.Enums.TransactionType Type { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
