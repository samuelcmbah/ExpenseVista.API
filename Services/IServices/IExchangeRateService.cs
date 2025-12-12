namespace ExpenseVista.API.Services.IServices
{
    public interface IExchangeRateService
    {
        Task<decimal> GetRateAsync(string fromCurrency, string toCurrency);
        Task<List<string>> GetSupportedCurrenciesAsync();
        Task<decimal?> GetCachedRateAsync(string fromCurrency, string toCurrency); 

    }
}
