using ExpenseVista.API.DTOs.Analytics.Exports;
using ExpenseVista.API.Services.Analytics;
using ExpenseVista.API.Services.IServices;
using ExpenseVista.API.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseVista.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportExportsController : BaseController
    {
        private readonly IAnalyticsService analyticsService;
        private readonly IFinancialReportExporter excelExporter;

        public ReportExportsController(IAnalyticsService analyticsService, IFinancialReportExporter excelExporter)
        {
            this.analyticsService = analyticsService;
            this.excelExporter = excelExporter;
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportAnalytics([FromBody] ExportRequestDto request)
        {
            var analytics = await analyticsService.GetAnalyticsAsync(request.Period, GetUserId());

            var exportModel = FinancialReportExportMapper.MapToExport(analytics);
            var fileBytes = excelExporter.Export(exportModel);

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ExpenseVista_Report_{request.Period}.xlsx"
            );
        }

    }
}
