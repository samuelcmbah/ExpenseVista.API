using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExpenseVista.API.Utilities
{
    //ensures that data is properly serialized
    public class DecimalTwoPlacesConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetDecimal();
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            // Always keep two decimal places, even for .00
            writer.WriteStringValue(value.ToString("F2", CultureInfo.InvariantCulture));
        }
    }

}
