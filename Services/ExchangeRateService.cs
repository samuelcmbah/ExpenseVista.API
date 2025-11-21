using ExpenseVista.API.DTOs.CurrencyExchange;
using ExpenseVista.API.Services.IServices;
using Microsoft.Extensions.Caching.Memory;

public class ExchangeRateService : IExchangeRateService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;

    public ExchangeRateService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<decimal> GetRateAsync(string fromCurrency, string toCurrency)
    {
        // Try reading from cache first
        var cacheKey = $"exchange_rates_{fromCurrency.ToUpper()}";

        if (!_cache.TryGetValue(cacheKey, out ExchangeRateResponse? cachedRates))
        {
            // Not cached → fetch from API
            var endpoint = $"https://open.er-api.com/v6/latest/{fromCurrency}";

            cachedRates = await _httpClient.GetFromJsonAsync<ExchangeRateResponse>(endpoint)
                ?? throw new Exception("Failed to fetch exchange rates. Response was null.");

            if (cachedRates.Result != "success")
                throw new Exception("Exchange rate API returned non-success result.");

            // Cache it for 6 hours
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6)
            };

            _cache.Set(cacheKey, cachedRates, cacheOptions);
        }

        // Now we are guaranteed to have data (from API or cache)
        if (!cachedRates!.Rates.TryGetValue(toCurrency, out var rate))
            throw new Exception($"Rate for {toCurrency} not found in cached data.");

        return rate;
    }

    public async Task<List<string>> GetSupportedCurrenciesAsync()
    {
        // We can reuse the cached USD data
        var rates = await _httpClient.GetFromJsonAsync<ExchangeRateResponse>("https://open.er-api.com/v6/latest/USD");

        if (rates == null || rates.Result != "success")
            throw new Exception("Failed to fetch supported currencies.");

        return rates.Rates.Keys.OrderBy(x => x).ToList();
    }
}
