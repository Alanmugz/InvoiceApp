

namespace InvoiceApp
open System
open System.IO
open Microsoft.FSharp.Core
open Microsoft.Office
open Microsoft.Office.Interop.Excel
open Microsoft.Office.Interop

module Excel = 
    
    let mutable row = 17

    let generateInvoice totalInvoiceAmountPerCurrencies (messageReceived : MessageType.InvoiceMessage) =

       let invoiceNumber = Random().Next(1000, 1000000000)
       let invoicingCurrencyCode = Convert.invoiceCurrencyIdToCurrencyCode messageReceived.InvoiceCurrency

       let app = new ApplicationClass(Visible = false) 

       // Create new file and get the first worksheet
       let workbook = app.Workbooks.Open(@"C:\Users\amulligan\Desktop\Book1.xlsx")
       // Note that worksheets are indexed from one instead of zero
       let worksheet = (workbook.Worksheets.[1] :?> Worksheet)

       //Invoice Currency
       worksheet.Cells.[5,3] <- invoicingCurrencyCode
       //Profit Margin
       worksheet.Cells.[6,3] <- messageReceived.ProfitMargin
       //Exchang Rate between CCS(EUR) and invoicing currency
       worksheet.Cells.[7,3] <- Http.getExchangeRates "EUR" <| invoicingCurrencyCode
       //Invoicing Time Peroid
       worksheet.Cells.[8,3] <- String.Format("{0} - {1}", messageReceived.DateFrom, messageReceived.DateTo.AddHours(23.0).AddMinutes(59.0).AddSeconds(59.9))

       let populateExcelDocument currencyCode totalPerCurrencyAfterEchange totalPerCurrencyBeforeExchange = 
           worksheet.Cells.[row,2] <- currencyCode
           worksheet.Cells.[row,3] <- totalPerCurrencyBeforeExchange
           worksheet.Cells.[row,4] <- Http.getExchangeRates <| invoicingCurrencyCode <| currencyCode
           worksheet.Cells.[row,5] <- totalPerCurrencyAfterEchange
           row <- row + 1

       totalInvoiceAmountPerCurrencies |> Seq.iter (fun (currencyCode, totalPerCurrencyAfterEchange, totalPerCurrencyBeforeExchange) -> populateExcelDocument currencyCode totalPerCurrencyAfterEchange totalPerCurrencyBeforeExchange)

       row <- 17

       let fileName = String.Format(@"C:\Users\amulligan\Desktop\CCS_Project\Invoice File\{0}", invoiceNumber)

       workbook.SaveAs(fileName)

       workbook.Close()