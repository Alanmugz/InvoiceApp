

namespace InvoiceApp

open Microsoft.Office.Interop

module Excel = 
    
    let mutable r = 1

    let doStuff linqExample =
        // Start Excel, Open a exiting file for input and create a new file for output
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

        r <- 1