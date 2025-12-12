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
    //tries to get a live rate
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
        {
            throw new Exception($"Rate for {toCurrency} not found in cached data.");
        }

        return rate;
    }

    //safely get a cached rate without an API call.
    public Task<decimal?> GetCachedRateAsync(string fromCurrency, string toCurrency)
    {
        var cacheKey = $"exchange_rates_{fromCurrency.ToUpper()}";

        if (_cache.TryGetValue(cacheKey, out ExchangeRateResponse? cachedRates) && cachedRates != null)
        {
            if (cachedRates.Rates.TryGetValue(toCurrency, out var rate))
            {
                return Task.FromResult<decimal?>(rate);
            }
        }

        // Return null if not found in cache
        return Task.FromResult<decimal?>(null);
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
