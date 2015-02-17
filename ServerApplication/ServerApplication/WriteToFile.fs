

namespace InvoiceApp
open System
open System.IO
open Microsoft.FSharp.Core
open Microsoft.Office
open Microsoft.Office.Interop.Excel
open Microsoft.Office.Interop

module Excel = 
    
    let mutable row = 17

    let printInvoice linqExample (message : Utilities.MessageTypes.InvoiceMessage) =

       let app = new ApplicationClass(Visible = true) 

       // Create new file and get the first worksheet
       let workbook = app.Workbooks.Open(@"C:\Users\amulligan\Desktop\Book1.xlsx")
       // Note that worksheets are indexed from one instead of zero
       let worksheet = (workbook.Worksheets.[1] :?> Worksheet)

       //Invoice Currency
       worksheet.Cells.[5,3] <- Utilities.getCurrencyCodeStr message.InvoiceCurrency
       //Exchange Rate
       worksheet.Cells.[6,3] <- message.ProfitMargin
       //Profit Margin
       worksheet.Cells.[7,3] <- Http.getRates "EUR" <| Utilities.getCurrencyCodeStr message.InvoiceCurrency
       //Invoicing Time Peroid
       worksheet.Cells.[8,3] <- String.Format("{0} - {1}", message.DateFrom, message.DateTo.AddHours(23.0).AddMinutes(59.0).AddSeconds(59.9))

       let stuff currencyCode totalPerCurrencyAfterEchange totalPerCurrencyBeforeExchange = 
           worksheet.Cells.[row,2] <- currencyCode
           worksheet.Cells.[row,3] <- totalPerCurrencyBeforeExchange
           worksheet.Cells.[row,4] <- Http.getRates <| Utilities.getCurrencyCodeStr message.InvoiceCurrency <| currencyCode
           worksheet.Cells.[row,5] <- totalPerCurrencyAfterEchange
           row <- row + 1

       linqExample |> Seq.iter (fun (currencyCode, totalPerCurrencyAfterEchange, totalPerCurrencyBeforeExchange) -> stuff currencyCode totalPerCurrencyAfterEchange totalPerCurrencyBeforeExchange)

       row <- 17

        (* Start Excel, Open a exiting file for input and create a new file for output
        let xlApp = new Excel.ApplicationClass()
        let xlWorkBookInput = xlApp.Workbooks.Open(@"C:\Users\amulligan\Desktop\Book1.xlsx")
        let xlWorkBookOutput = xlApp.Workbooks.Add()
        xlApp.Visible <- true
 
        // Open input's 'Sheet1' and create a new worksheet in file.xlsx
        let xlWorkSheetInput = xlWorkBookInput.Worksheets.["Sheet1"] :?> Excel.Worksheet
        let xlWorkSheetOutput = xlWorkBookOutput.Worksheets.[1] :?> Excel.Worksheet
        xlWorkSheetOutput.Name <- "OutputSheet1"

        let stuff x y = 
            xlWorkSheetOutput.Cells.[r,1] <- x
            xlWorkSheetOutput.Cells.[r,2] <- y
            r <- r + 1
        
        linqExample |> Seq.iter (fun (x, y) -> stuff x y)

        xlWorkSheetOutput.Cells.[r,1] <- "Total"
        xlWorkSheetOutput.Cells.[r,2] <- 

        r <- 1*)