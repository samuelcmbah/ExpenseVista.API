using ExpenseVista.API.DTOs.Analytics.Exports;

namespace ExpenseVista.API.Services.IServices
{
    public interface IFinancialReportExporter
    {
        byte[] Export(FinancialReportExport report);
    }
}
