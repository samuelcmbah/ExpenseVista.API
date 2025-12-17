using ClosedXML.Excel;
using ExpenseVista.API.DTOs.Analytics.Exports;
using ExpenseVista.API.Services.IServices;
using System.Globalization;

namespace ExpenseVista.API.Services.Exports
{
    public class FinancialReportExcelExporter : IFinancialReportExporter
    {

        private const string NairaFormat = "₦ #,##0.00";
        private const double CurrencyColumnWidth = 18;
        private static readonly XLColor AccentColor = XLColor.SeaGreen;

        //PUBLIC ACCESS METHOD
        public byte[] Export(FinancialReportExport report)
        {
            var originalCulture = CultureInfo.CurrentCulture;
            var originalUICulture = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
                CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

                //create a workbook and add all the sheets using separate private methods
                using var workbook = new XLWorkbook();

                workbook.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                AddOverviewSheet(workbook, report);
                AddBudgetBreakdownSheet(workbook, report.BudgetBreakdown);
                AddCategorySpendingSheet(workbook, report.CategorySpending);
                AddIncomeVsExpensesSheet(workbook, report.MonthlyIncomeVsExpenses);
                AddTransactionsSheet(workbook, report.Transactions);

                //SAVE the workbook to a stream and return the byte array, a standard format for returning files in controllers
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUICulture;
            }
        }
        //HELPER METHODS
        private static IXLCell WriteKeyValue(IXLWorksheet ws, ref int row, string label, object value, bool isCurrency = false, bool isPercentage = false)
        {
            ws.Cell(row, 1).Value = label;
            ws.Cell(row, 1).Style.Font.SetBold();

            var valueCell = ws.Cell(row, 2);
            valueCell.Value = XLCellValue.FromObject(value);

            if (isCurrency)
            {
                valueCell.Style.NumberFormat.Format = NairaFormat;
            }
            else if (isPercentage)
            {
                valueCell.Style.NumberFormat.Format = "0.00'%'";
            }
            else if (value is int)
            {
                valueCell.Style.NumberFormat.Format = "#,##0";
            }

            row++;
            return valueCell; // Return the cell so we can style it further
        }

        private static void ApplyCustomHeaderStyle(IXLTable table)
        {
            // Set the background color to our defined accent green
            table.HeadersRow().Style.Fill.SetBackgroundColor(AccentColor);

            // Set the font color to white and make it bold for readability
            table.HeadersRow().Style.Font.SetFontColor(XLColor.White);
            table.HeadersRow().Style.Font.SetBold();
        }

        private static void AddOverviewSheet(XLWorkbook workbook, FinancialReportExport report)
        {
            var ws = workbook.Worksheets.Add("Overview");
            ws.Column(1).Width = 25; // Give labels more space
            ws.Column(2).Width = 25; // Give values space

            // -- Header --
            ws.Cell("A1").Value = "ExpenseVista Financial Report";
            ws.Cell("A1").Style.Font.SetBold();
            ws.Cell("A1").Style.Font.FontSize = 16;
            ws.Cell("A1").Style.Font.FontColor = AccentColor; // Apply accent color
            ws.Range("A1:B1").Merge(); // Merge cells for a cleaner title

            // -- User and Date Info --
            var row = 3;
            WriteKeyValue(ws, ref row, "Report For", report.UserName);
            WriteKeyValue(ws, ref row, "Email", report.UserEmail);
            WriteKeyValue(ws, ref row, "Time Period", report.TimePeriod);
            WriteKeyValue(ws, ref row, "Date", $"{report.StartDate:dd MMM yyyy} - {report.EndDate:dd MMM yyyy}");

            row++;

            // -- Financial Summary Section --
            ws.Cell(row, 1).Value = "Financial Summary";
            ws.Cell(row, 1).Style.Font.SetBold();
            ws.Cell(row, 1).Style.Font.FontSize = 12;
            ws.Cell(row, 1).Style.Font.FontColor = XLColor.White; // White text
            ws.Range(row, 1, row, 2).Merge().Style.Fill.SetBackgroundColor(AccentColor);
            row++;

            WriteKeyValue(ws, ref row, "Total Income", report.Overview.TotalIncome, isCurrency: true);
            WriteKeyValue(ws, ref row, "Total Expenses", report.Overview.TotalExpenses, isCurrency: true);

            // Conditional Formatting for Net Balance
            var netBalanceCell = WriteKeyValue(ws, ref row, "Cash Flow", report.Overview.NetBalance, isCurrency: true);
            if (report.Overview.NetBalance >= 0)
            {
                netBalanceCell.Style.Font.SetFontColor(XLColor.Green);
            }
            else
            {
                netBalanceCell.Style.Font.SetFontColor(XLColor.Red);
            }

            row++; // Add a space between sections

            // -- Key Insights Section --
            ws.Cell(row, 1).Value = "Key Insights";
            ws.Cell(row, 1).Style.Font.SetBold();
            ws.Cell(row, 1).Style.Font.FontSize = 12;
            ws.Cell(row, 1).Style.Font.FontColor = XLColor.White;
            ws.Range(row, 1, row, 2).Merge().Style.Fill.SetBackgroundColor(AccentColor);
            row++;

            WriteKeyValue(ws, ref row, "Top Spending Category", report.Overview.TopSpendingCategory);
            WriteKeyValue(ws, ref row, "Top Spending Amount", report.Overview.TopSpendingAmount);
            WriteKeyValue(ws, ref row, "Total Transactions", report.Overview.TotalTransactions);
            WriteKeyValue(ws, ref row, "Income Transactions", report.Overview.IncomeTransactions);
            WriteKeyValue(ws, ref row, "Expense Transactions", report.Overview.ExpenseTransactions);

            //ws.Columns().AdjustToContents();
        }

        private static void AddBudgetBreakdownSheet(XLWorkbook workbook, IReadOnlyList<MonthlyBudgetExport> data)
        {
            var ws = workbook.Worksheets.Add("Budget Breakdown");

            if (!data.Any())
            {
                ws.Cell("A1").Value = "No budget information available for this period.";
                ws.Column("A").AdjustToContents();
                return;
            }

            ws.Cell(1, 1).Value = "Month";
            ws.Cell(1, 2).Value = "Budget";
            ws.Cell(1, 3).Value = "Spent";
            ws.Cell(1, 4).Value = "Balance";
            ws.Range("A1:D1").Style.Font.SetBold();
            ws.Range("A1:D1").Style.Font.FontColor = XLColor.White;
            ws.Range("A1:D1").Style.Fill.SetBackgroundColor(AccentColor);

            var row = 2;
            foreach (var item in data)
            {
                ws.Cell(row, 1).Value = item.Month;

                // Handle nullable budget cleanly
                if (item.Budget.HasValue && item.Balance.HasValue)
                {
                    ws.Cell(row, 2).Value = item.Budget.Value;
                    ws.Cell(row, 3).Value = item.Spent;
                    ws.Cell(row, 4).Value = item.Balance.Value;
                }
                else
                {
                    ws.Range(row, 2, row, 4).Merge().SetValue("No budget set for this month");
                    ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, 2).Style.Font.SetItalic();
                }
                row++;
            }

            // Apply formatting
            ws.Columns("B:D").Style.NumberFormat.Format = NairaFormat;
            ws.SheetView.FreezeRows(1);

            ws.Column("A").AdjustToContents();
            ws.Columns("B:D").Width = CurrencyColumnWidth;
        }

        private static void AddCategorySpendingSheet(XLWorkbook workbook, IReadOnlyList<CategorySpendingExport> data)
        {
            var ws = workbook.Worksheets.Add("Spending by Category");
          

            // Insert the data as a table directly
            if (data.Any())
            {
                var table = ws.Cell(1, 1).InsertTable(data);
                table.Theme = XLTableTheme.TableStyleLight8;
                ApplyCustomHeaderStyle(table);
                table.ShowTotalsRow = true;

                // Specify which columns get a total. "Sum" is the default.
                table.Field("AmountSpent").TotalsRowFunction = XLTotalsRowFunction.Sum;
                table.Field("Percentage").TotalsRowFunction = XLTotalsRowFunction.Sum;

                ws.Column("B").Style.NumberFormat.Format = NairaFormat; // Amount Spent
                ws.Column("C").Style.NumberFormat.Format = "0.00%";   // Percentage

                ws.SheetView.FreezeRows(1);

                ws.Column("A").AdjustToContents(); // Adjust Category
                ws.Column("B").Width = CurrencyColumnWidth; // Set fixed width for AmountSpent
                ws.Column("C").AdjustToContents(); //
            }
            else
            {
                // Handle the case with no data gracefully
                ws.Cell("A1").Value = "No category spending data for this period.";
                ws.Column("A").AdjustToContents(); 

            }
        }
        private static void AddIncomeVsExpensesSheet(XLWorkbook workbook, IReadOnlyList<MonthlyIncomeExpenseExport> data)
        {
            var ws = workbook.Worksheets.Add("Income vs Expenses");

            if (!data.Any())
            {
                ws.Cell("A1").Value = "No monthly data available for this period.";
                ws.Columns().AdjustToContents();
                return;
            }

            // This creates the table and headers automatically from your DTO properties.
            var table = ws.Cell(1, 1).InsertTable(data);
            table.Theme = XLTableTheme.TableStyleLight8;
            ApplyCustomHeaderStyle(table);

            // Add totals row for a better user experience
            table.ShowTotalsRow = true;
            table.Field("Month").TotalsRowLabel = "Totals:";
            table.Field("Income").TotalsRowFunction = XLTotalsRowFunction.Sum;
            table.Field("Expenses").TotalsRowFunction = XLTotalsRowFunction.Sum;

            // Apply the Naira format to the Income (B) and Expenses (C) columns
            ws.Columns("B:C").Style.NumberFormat.Format = NairaFormat;

            ws.SheetView.FreezeRows(1);
            ws.Column("A").AdjustToContents(); // Adjust Month column
            ws.Columns("B:C").Width = CurrencyColumnWidth; // Set fixed width for Income & Expenses
        }
        private static void AddTransactionsSheet(XLWorkbook workbook, IReadOnlyList<TransactionExport> data)
        {
            var ws = workbook.Worksheets.Add("All Transactions");

            if (!data.Any())
            {
                ws.Cell("A1").Value = "No transactions found for the selected period.";
                ws.Columns().AdjustToContents();
                return;
            }

            var table = ws.Cell(1, 1).InsertTable(data);
            table.Theme = XLTableTheme.TableStyleLight8;
            ApplyCustomHeaderStyle(table);

            //format and allow totals for specific columns
            table.ShowTotalsRow = true;
            table.Field("Date").TotalsRowLabel = "Totals:";
            table.Field("Income").TotalsRowFunction = XLTotalsRowFunction.Sum;
            table.Field("Expense").TotalsRowFunction = XLTotalsRowFunction.Sum;

            // Apply specific column formatting
            ws.Column("A").Style.DateFormat.Format = "dd/mm/yyyy"; // Date column
            ws.Columns("D:E").Style.NumberFormat.Format = NairaFormat;

            ws.SheetView.FreezeRows(1);
            ws.Column("A").AdjustToContents(); // Adjust Date.
            ws.Column("B").Width = CurrencyColumnWidth;
            ws.Columns("C").AdjustToContents(); //  Category
            ws.Columns("D:E").Width = CurrencyColumnWidth;
        }
    }
}
