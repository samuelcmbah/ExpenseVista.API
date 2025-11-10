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
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                {
                    return result;
                }

                throw new JsonException($"Invalid decimal format: {stringValue}");
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDecimal();
            }

            throw new JsonException($"Unexpected token parsing decimal. Token: {reader.TokenType}");
        }


        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            // Always keep two decimal places, even for .00
            writer.WriteStringValue(value.ToString("F2", CultureInfo.InvariantCulture));
        }
    }

}
