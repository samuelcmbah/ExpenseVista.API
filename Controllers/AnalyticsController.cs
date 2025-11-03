using ExpenseVista.API.Controllers;
using ExpenseVista.API.DTOs.Analytics;
using ExpenseVista.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseVista.Api.Controllers
{
    [ApiController]
    [Route("api/analytics")]
    [Authorize]
    public class AnalyticsController : BaseController
    {
        private readonly IAnalyticsService analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            this.analyticsService = analyticsService;
        }
        [HttpGet]
        public async Task<ActionResult<FinancialDataDTO>> GetAnalytics([FromQuery] string period)
        {
            try
            {

            // Validate input
            var validPeriods = new[] { "This Month", "Last 3 Months", "Last 6 Months", "This Year" };
            if (!validPeriods.Contains(period))
                return BadRequest("Invalid period parameter.");
            var userId = GetUserId();

           var data = await analyticsService.GetAnalyticsAsync(period, userId);

            return Ok(data);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

       
    }
}
