using ExpenseVista.API.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseVista.API.Controllers
{
    [ApiController]
    [Route("api/currency")]
    public class CurrencyController : ControllerBase
    {
        private readonly IExchangeRateService _exchangeService;

        public CurrencyController(IExchangeRateService exchangeService)
        {
            _exchangeService = exchangeService;
        }

        [HttpGet("supported")]
        public async Task<IActionResult> GetSupportedCurrencies()
        {
            var currencies = await _exchangeService.GetSupportedCurrenciesAsync();
            return Ok(currencies);
        }

        [HttpGet("rate")]
        public async Task<IActionResult> GetRate([FromQuery] string from, [FromQuery] string to)
        {
            var rate = await _exchangeService.GetRateAsync(from, to);
            return Ok(rate);
        }
    }
}
