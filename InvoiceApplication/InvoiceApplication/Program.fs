// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

open FSharp.Data
open Microsoft.FSharp.Core
open Microsoft.Office
open Microsoft.Office.Interop.Excel
open Microsoft.Office.Interop
open Npgsql
open System
open System.Collections.Generic
open System.Diagnostics
open System.IO
open System.Threading


module Json = 
    type Sample = JsonProvider<"""  {"InvoiceNumber": "1-3456789",
                                      "InvoiceDate": "21/12/2015",
                                      "InvoiceCurrency": "USD",
                                      "InvoiceProfitMargin": 2.3,
                                      "InvoicePaymentCurrencyToEuroExchangeRate": 0.8658745,
                                      "InvoicePeroid": "01/01/2015 00:00:00 - 01/02/2015 23:59:59",
                                      "Transaction": [
                                        {
                                          "CurrencyCode": "USD",
                                          "TotalPerCurrencyBeforeExchange": 12.36,
                                          "ExchangeRate": 56.32,
                                          "TotalPerCurrencyAfterExchange": 17.23
                                        }
                                      ],
                                      "TransactionForPeroid": 254584.01,
                                      "InvoicingAmountWithProfitMarginApplied": 8452.01,
                                      "InvoicingAmountWithProfitMarginAppliedInEuro": 8452.01
                                    } """>

module Database = 
    let connectionString = "Server = localhost; Port = 5432; Database = InvoiceApplication; User Id = postgres; Password = y6j5atu5 ; CommandTimeout = 40;"

    let openConnection = new Npgsql.NpgsqlConnection(connectionString)

    let queryString = "SELECT \"Number\", \"Json\"  
                       FROM \"Invoice\"
                       WHERE \"Number\" = '1-750525679'"

module Excel = 

    let startRow = 17

    let generateInvoice invoiceJson =

        let parsedJson = Json.Sample.Parse(invoiceJson)
        let invoiceNumber = parsedJson.InvoiceNumber
        let numberOfTransactionRequired = Convert.ToInt32(parsedJson.Transaction.Length)

        let seqOfTransaction = seq {0 .. (numberOfTransactionRequired-1)}
        let rowSeq = seq{ startRow .. (startRow + (numberOfTransactionRequired)) }

        let app = new ApplicationClass(Visible = false) 
        // Create new file and get the first worksheet
        let workbook = app.Workbooks.Open(@"C:\Users\amulligan\Desktop\Invoice File\InvoiceTemplate.xlsx")
        // Note that worksheets are indexed from one instead of zero
        let worksheet = (workbook.Worksheets.[1] :?> Worksheet)

        //Invoice Number
        worksheet.Cells.[2,3] <- invoiceNumber
        //Invoice Currency
        worksheet.Cells.[5,3] <- parsedJson.InvoiceCurrency
        //Profit Margin
        worksheet.Cells.[6,3] <- parsedJson.InvoiceProfitMargin
        //Exchang Rate between CCS(EUR) and invoicing currency
        worksheet.Cells.[7,3] <- parsedJson.InvoicePaymentCurrencyToEuroExchangeRate
        //Invoicing Time Peroid
        worksheet.Cells.[8,3] <- parsedJson.InvoicePeroid
        //invoicingCurrencyTotalAfterProfitMarginSplit 
        worksheet.Cells.[4,5] <- parsedJson.InvoicingAmountWithProfitMarginApplied
        //ccsProfitInEuro
        worksheet.Cells.[7,5] <- parsedJson.InvoicingAmountWithProfitMarginAppliedInEuro
        //invoicingCurrencyTotalBeforeExchange
        worksheet.Cells.[19 + (numberOfTransactionRequired),5] <- parsedJson.TransactionForPeroid

        let populateExcelDocument currencyCode totalPerCurrencyBeforeExchange exchangeRate totalPerCurrencyAfterExchange row = 
            worksheet.Cells.[row,2] <- currencyCode
            worksheet.Cells.[row,3] <- totalPerCurrencyBeforeExchange
            worksheet.Cells.[row,4] <- exchangeRate
            worksheet.Cells.[row,5] <- totalPerCurrencyAfterExchange
           
        seqOfTransaction
        |> Seq.filter (fun n -> parsedJson.Transaction.[n].CurrencyCode <> parsedJson.InvoiceCurrency)
        |> Seq.iter2(fun count row -> populateExcelDocument parsedJson.Transaction.[count].CurrencyCode 
                                                            parsedJson.Transaction.[count].TotalPerCurrencyBeforeExchange 
                                                            parsedJson.Transaction.[count].ExchangeRate 
                                                            parsedJson.Transaction.[count].TotalPerCurrencyAfterExchange 
                                                            row ) <| rowSeq
        try         
            let fileName = String.Format(@"C:\Users\amulligan\Desktop\Invoice File\{0}-Copy",invoiceNumber)
            workbook.SaveAs(fileName)
        finally
            workbook.Close()
    
    let prepairRowInInvoiceTemplate (numberOfTransactionInSeq: int) = 
        let processStartInfo = new ProcessStartInfo(@"C:\Users\amulligan\Desktop\CCS_Project\ExcelPreperation.exe")
        processStartInfo.Arguments <- numberOfTransactionInSeq.ToString()
        processStartInfo.UseShellExecute <- false
        System.Diagnostics.Process.Start(processStartInfo)

module Main =

    let queryDatabase (connection: NpgsqlConnection) (queryString: string) =
      [ let command = new NpgsqlCommand(queryString, connection)
        let dataReader = command.ExecuteReader()
        while dataReader.Read() do
            yield dataReader.GetString(1) ]

    let conn = Database.openConnection
    conn.Open() 

    try
        let jsonData = queryDatabase <| conn <| Database.queryString
        let invoiceJson = String.Format(""" {0} """, jsonData.[0])

        let parsedJson = Json.Sample.Parse(invoiceJson)
    
        let numberOfTransaction = (Convert.ToInt32(parsedJson.Transaction.Length))

        Excel.prepairRowInInvoiceTemplate numberOfTransaction |> ignore

        Thread.Sleep(2000)  

        Excel.generateInvoice invoiceJson

    finally
        conn.Close()