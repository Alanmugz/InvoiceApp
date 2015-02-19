
namespace InvoiceApp

open Microsoft.FSharp.Core
open Microsoft.Office
open Microsoft.Office.Interop.Excel
open Microsoft.Office.Interop
open System
open System.Diagnostics
open System.IO

module Excel = 
    
    
    let row = 17

    let generateInvoice totalInvoiceAmountPerCurrencies (messageReceived : MessageType.InvoiceMessage) 
                        invoicingCurrencyTotalBeforeExchange invoicingCurrencyTotalAfterProfitMarginSplit 
                        ccsProfitInEuro selectedInvoicingCurrencyCode numberOfRowsRequiredInExcelTable =

       let a = seq{ row .. (row + numberOfRowsRequiredInExcelTable) }
       let invoiceNumber = String.Format("{0}-{1}",messageReceived.MerchantId, Random().Next(1000, 1000000000))
       let invoicingCurrencyCode = Convert.invoiceCurrencyIdToCurrencyCode messageReceived.InvoiceCurrency

       let app = new ApplicationClass(Visible = false) 

       // Create new file and get the first worksheet
       let workbook = app.Workbooks.Open(@"C:\Users\amulligan\Desktop\Invoice File\InvoiceTemplate.xlsx")
       // Note that worksheets are indexed from one instead of zero
       let worksheet = (workbook.Worksheets.[1] :?> Worksheet)

       //Invoice Number
       worksheet.Cells.[2,3] <- invoiceNumber
       //Invoice Currency
       worksheet.Cells.[5,3] <- invoicingCurrencyCode
       //Profit Margin
       worksheet.Cells.[6,3] <- messageReceived.ProfitMargin
       //Exchang Rate between CCS(EUR) and invoicing currency
       worksheet.Cells.[7,3] <- Http.getExchangeRates "EUR" <| invoicingCurrencyCode
       //Invoicing Time Peroid
       worksheet.Cells.[8,3] <- String.Format("{0} - {1}", messageReceived.DateFrom, messageReceived.DateTo.AddHours(23.0).AddMinutes(59.0).AddSeconds(59.9))
       //invoicingCurrencyTotalAfterProfitMarginSplit 
       worksheet.Cells.[4,5] <- invoicingCurrencyTotalAfterProfitMarginSplit
       //ccsProfitInEuro
       worksheet.Cells.[7,5] <- ccsProfitInEuro
       //invoicingCurrencyTotalBeforeExchange
       worksheet.Cells.[18 + numberOfRowsRequiredInExcelTable,5] <- invoicingCurrencyTotalBeforeExchange

       let populateExcelDocument currencyCode totalPerCurrencyAfterExchange totalPerCurrencyBeforeExchange x = 
           worksheet.Cells.[x,2] <- currencyCode
           worksheet.Cells.[x,3] <- totalPerCurrencyBeforeExchange
           worksheet.Cells.[x,4] <- Http.getExchangeRates <| invoicingCurrencyCode <| currencyCode
           worksheet.Cells.[x,5] <- totalPerCurrencyAfterExchange
           

       totalInvoiceAmountPerCurrencies 
       |> Seq.filter (fun (currencyCode, totalPerCurrencyAfterExchange, totalPerCurrencyBeforeExchange) -> currencyCode <> selectedInvoicingCurrencyCode)
       |> Seq.iter2 (fun (currencyCode, totalPerCurrencyAfterEchange, totalPerCurrencyBeforeExchange) x -> populateExcelDocument currencyCode totalPerCurrencyAfterEchange totalPerCurrencyBeforeExchange x) <| a


       let fileName = String.Format(@"C:\Users\amulligan\Desktop\Invoice File\{0}",invoiceNumber)

       workbook.SaveAs(fileName)

       workbook.Close()

    let prepairRowInInvoiceTemplate (numberOfTransactionInSeq: int) = 
       let psi = new ProcessStartInfo(@"C:\Users\amulligan\Desktop\CCS_Project\ExcelPreperation.exe")
       psi.Arguments <- numberOfTransactionInSeq.ToString()
       psi.UseShellExecute <- false
       System.Diagnostics.Process.Start(psi)
