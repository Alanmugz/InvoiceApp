

namespace InvoiceApp

open System.IO
open Microsoft.Office
open Microsoft.Office.Interop.Excel
open Microsoft.Office.Interop

module Excel = 
    
    let mutable row = 16

    let printInvoice linqExample profitMargin selectedInvoicingCurrencyCode =

       let app = new ApplicationClass(Visible = true) 

       // Create new file and get the first worksheet
       let workbook = app.Workbooks.Open(@"C:\Users\amulligan\Desktop\Book1.xlsx")
       // Note that worksheets are indexed from one instead of zero
       let worksheet = (workbook.Worksheets.[1] :?> Worksheet)

       //ExchangeRate
       worksheet.Cells.[5,3] <- selectedInvoicingCurrencyCode
       //ExchangeRate
       worksheet.Cells.[6,3] <- profitMargin
       //ProfitMargin
       worksheet.Cells.[7,3] <- Http.getRates "EUR" selectedInvoicingCurrencyCode

       let stuff x y = 
           worksheet.Cells.[row,3] <- x
           worksheet.Cells.[row,5] <- y
           row <- row + 1

       linqExample |> Seq.iter (fun (x, y) -> stuff x y)

       row <- 16

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