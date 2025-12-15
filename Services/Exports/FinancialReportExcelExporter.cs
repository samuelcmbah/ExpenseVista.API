using ClosedXML.Excel;
using ExpenseVista.API.DTOs.Analytics.Exports;
using ExpenseVista.API.Services.IServices;

namespace ExpenseVista.API.Services.Exports
{
    public class FinancialReportExcelExporter : IFinancialReportExporter
    {
        //PUBLIC ACCESS METHOD
        public byte[] Export(FinancialReportExport report)
        {
            //create a workbook and add all the sheets using separate private methods
            using var workbook = new XLWorkbook();

            AddOverviewSheet(workbook, report);
            AddCategorySpendingSheet(workbook, report.CategorySpending);
            AddIncomeVsExpensesSheet(workbook, report.MonthlyIncomeVsExpenses);
            AddFinancialTrendsSheet(workbook, report.FinancialTrends);

            //SAVE the workbook to a stream and return the byte array, a standard format for returning files in controllers
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
        //HELPER METHODS
        private static void WriteKeyValue(IXLWorksheet ws, ref int row, string label, object value)
        {
            ws.Cell(row, 1).Value = label;
            ws.Cell(row, 1).Style.Font.Bold = true;

            ws.Cell(row, 2).Value = XLCellValue.FromObject(value);
            row++;
        }

        private static void WriteMonthlyHeader(IXLWorksheet ws)
        {
            ws.Cell(1, 1).Value = "Month";
            ws.Cell(1, 2).Value = "Income";
            ws.Cell(1, 3).Value = "Expenses";

            ws.Range("A1:C1").Style.Font.Bold = true;
        }

        private static void FormatMonthlySheet(IXLWorksheet ws)
        {
            ws.Columns(2, 3).Style.NumberFormat.Format = "#,##0.00";
            ws.Columns().AdjustToContents();
        }



        private static void AddOverviewSheet(XLWorkbook workbook, FinancialReportExport report)
        {
            var ws = workbook.Worksheets.Add("Overview");

            // Set a large, bold title in A1
            ws.Cell("A1").Value = "ExpenseVista Financial Report";
            ws.Cell("A1").Style.Font.Bold = true;
            ws.Cell("A1").Style.Font.FontSize = 16;

            ws.Cell("A3").Value = "Time Period";
            ws.Cell("A3").Style.Font.Bold = true;
            ws.Cell("B3").Value = report.TimePeriod;

            var row = 5;

            WriteKeyValue(ws, ref row, "Total Income", report.Overview.TotalIncome);
            WriteKeyValue(ws, ref row, "Total Expenses", report.Overview.TotalExpenses);
            WriteKeyValue(ws, ref row, "Cash Flow", report.Overview.NetBalance);

            row++;

            WriteKeyValue(ws, ref row, "Budget Total", report.Overview.BudgetTotal);
            WriteKeyValue(ws, ref row, "Budget Used (%)", report.Overview.BudgetUsedPercentage);
            WriteKeyValue(ws, ref row, "Budget Balance", report.Overview.BudgetBalance);

            row++;

            WriteKeyValue(ws, ref row, "Top Spending Category", report.Overview.TopSpendingCategory);
            WriteKeyValue(ws, ref row, "Top Spending Amount", report.Overview.TopSpendingAmount);

            row++;

            WriteKeyValue(ws, ref row, "Total Transactions", report.Overview.TotalTransactions);
            WriteKeyValue(ws, ref row, "Income Transactions", report.Overview.IncomeTransactions);
            WriteKeyValue(ws, ref row, "Expense Transactions", report.Overview.ExpenseTransactions);

            ws.Columns().AdjustToContents();
        }

        private static void AddCategorySpendingSheet(XLWorkbook workbook, IReadOnlyList<CategorySpendingExport> data)
        {
            var ws = workbook.Worksheets.Add("Spending by Category");
            //write headers
            ws.Cell(1, 1).Value = "Category";
            ws.Cell(1, 2).Value = "Amount Spent";
            ws.Cell(1, 3).Value = "Percentage";

            //iterate through the list of dto and put in appropriate cells
            ws.Range("A1:C1").Style.Font.Bold = true;

            var row = 2;
            foreach (var item in data)
            {
                ws.Cell(row, 1).Value = item.Category;
                ws.Cell(row, 2).Value = item.AmountSpent;
                ws.Cell(row, 3).Value = item.Percentage;
                row++;
            }

            ws.Columns(2, 3).Style.NumberFormat.Format = "#,##0.00"; // Sets columns 2 and 3 to currency/decimal format
            ws.Columns().AdjustToContents();
        }

        private static void AddIncomeVsExpensesSheet(XLWorkbook workbook, IReadOnlyList<MonthlyIncomeExpenseExport> data)
        {
            var ws = workbook.Worksheets.Add("Income vs Expenses");

            WriteMonthlyHeader(ws);

            var row = 2;
            foreach (var item in data)
            {
                ws.Cell(row, 1).Value = item.Month;
                ws.Cell(row, 2).Value = item.Income;
                ws.Cell(row, 3).Value = item.Expenses;
                row++;
            }

            FormatMonthlySheet(ws);
        }

        private static void AddFinancialTrendsSheet(XLWorkbook workbook,IReadOnlyList<MonthlyIncomeExpenseExport> data)
        {
            var ws = workbook.Worksheets.Add("Financial Trends");

            WriteMonthlyHeader(ws);

            var row = 2;
            foreach (var item in data)
            {
                ws.Cell(row, 1).Value = item.Month;
                ws.Cell(row, 2).Value = item.Income;
                ws.Cell(row, 3).Value = item.Expenses;
                row++;
            }

            FormatMonthlySheet(ws);
        }

    }
}
