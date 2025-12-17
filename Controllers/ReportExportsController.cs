using ExpenseVista.API.DTOs.Analytics.Exports;
using ExpenseVista.API.Models;
using ExpenseVista.API.Services.Analytics;
using ExpenseVista.API.Services.IServices;
using ExpenseVista.API.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseVista.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportExportsController : BaseController
    {
        private readonly IAnalyticsService analyticsService;
        private readonly IFinancialReportExporter excelExporter;
        private readonly UserManager<ApplicationUser> userManager;

        public ReportExportsController(IAnalyticsService analyticsService, IFinancialReportExporter excelExporter, UserManager<ApplicationUser> userManager)
        {
            this.analyticsService = analyticsService;
            this.excelExporter = excelExporter;
            this.userManager = userManager;
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportAnalytics([FromBody] ExportRequestDto request)
        {
            var userId = GetUserId();
            var currentUser = await userManager.FindByIdAsync(userId);
            if (currentUser == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }
            var userName = $"{currentUser.FirstName} {currentUser.LastName}";
            var userEmail = currentUser.Email;
            var analytics = await analyticsService.GetAnalyticsAsync(request.Period, userId);

            var exportModel = FinancialReportExportMapper.MapToExport(analytics, userName, userEmail);
            var fileBytes = excelExporter.Export(exportModel);

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ExpenseVista_Report_{request.Period}.xlsx"
            );
        }

    }
}
