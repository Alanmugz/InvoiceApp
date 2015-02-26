using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication7
{
    class Program
    {
        static void Main(string[] args)
        {
            //int numberOfTransactions = 16;
            
            int numberOfTransactions = Convert.ToInt32(args[0]);
            int startRow = 16;
            int totalRows = startRow + numberOfTransactions;

            Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
            string path = @"C:\Users\amulligan\Desktop\CCS_Project\template.xlsx";
            excelApp.Workbooks.Open(path);
            excelApp.DisplayAlerts = false;
            excelApp.Visible = false;
            // Get Worksheet
            Microsoft.Office.Interop.Excel.Worksheet worksheet = excelApp.Worksheets[1];

            copyAndInsertTotalFooter(worksheet, totalRows);

            createTableWithHeaders(worksheet, totalRows, excelApp);

            worksheet.SaveAs(@"C:\Users\amulligan\Desktop\Invoice File\InvoiceTemplate");
            excelApp.Quit();
        }

        private static void createTableWithHeaders(Worksheet _worksheet, int _totalRows, Application _excelApp)
        {
            Microsoft.Office.Interop.Excel.Range _SourceRange = (Microsoft.Office.Interop.Excel.Range)_worksheet.get_Range("B16", "E" + _totalRows);

            _SourceRange.Worksheet.ListObjects.Add(XlListObjectSourceType.xlSrcRange,
                                                  _SourceRange, System.Type.Missing, XlYesNoGuess.xlYes, System.Type.Missing).Name = "Invoice";
            _SourceRange.Select();
            _SourceRange.Worksheet.ListObjects["Invoice"].TableStyle = "TableStyleMedium2";

            String[] _strArray = new String[] { "Payment Currency", "Payment Margin Total/Currency", "Exchange Rate   ", "Total/Currency(Exchange Applied) " };
            Range rng = _excelApp.get_Range("B16", "E16");
            rng.Value = _strArray;
        }

        private static void copyAndInsertTotalFooter(Worksheet _worksheet, int _totalRows)
        {
            Microsoft.Office.Interop.Excel.Range _copyRange = _worksheet.Range["B165:E168"];
            Microsoft.Office.Interop.Excel.Range _insertRange = _worksheet.Range["B" + (_totalRows + 3) + ":E" + (_totalRows + 6) + ""];
            _insertRange.Insert(Microsoft.Office.Interop.Excel.XlInsertShiftDirection.xlShiftToRight, _copyRange.Cut());
        }
    }
}
