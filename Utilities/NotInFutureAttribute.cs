using System.ComponentModel.DataAnnotations;

namespace ExpenseVista.API.Utilities
{
    public class NotInFutureAttribute : ValidationAttribute
    {

        public NotInFutureAttribute()
        {
            ErrorMessage = "Transaction date cannot be in the future.";
        }

        public override bool IsValid(object? value)
        {
            if (value is DateTime date)
            {
                return date <= DateTime.UtcNow;
            }
            return true;
        }
    }
}
