using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ExpenseVista.API.Data
{
    public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter()
            : base(
                // On Writing to DB: Convert any DateTime to UTC
                v => v.ToUniversalTime(),
                // On Reading from DB: Assume the DateTime is UTC and stamp it as such
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
        {
        }
    }
}
