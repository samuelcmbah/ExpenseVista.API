using ExpenseVista.API.DTOs.Analytics;
using ExpenseVista.API.Services;
using ExpenseVista.API.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseVista.API.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : BaseController
    {
        private readonly IDashboardService dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            this.dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardDTO>> GetDashboard([FromQuery] DateTime month)
        {
            var userId = GetUserId();
            try
            {
                var dashboardData = await dashboardService.GetDashboardAsync(userId, month);
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
