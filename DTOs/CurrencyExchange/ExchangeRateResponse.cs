namespace ExpenseVista.API.DTOs.CurrencyExchange
{
    public class ExchangeRateResponse
    {
        public string Result { get; set; } = string.Empty;
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }
}
