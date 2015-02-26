
namespace InvoiceApp

open Npgsql
open System
open System.Collections.Generic
open System.Linq

module Calculate = 

    let displayGeneratedData getTotalInvoiceAmountPerCurrency selectedInvoicingCurrencyCode profitMargin totalInvocieAmountInEuro consoleIsEnabled () =             
        match consoleIsEnabled with 
        | true -> 
            Console.Clear()

            getTotalInvoiceAmountPerCurrency 
                |> Seq.filter (fun (currencyCode, totalPerCurrencyAfterExchange, totalPerCurrencyBeforeExchange) -> currencyCode <> selectedInvoicingCurrencyCode)
                |> Seq.iter (fun (currencyCode, totalPerCurrencyAfterExchange, totalPerCurrencyBeforeExchange) -> 
                printf "%s - %10s %-10O\n" currencyCode selectedInvoicingCurrencyCode totalPerCurrencyAfterExchange)

            printf "----------------------\n" 

            let getTotalInvoiceAmount (x: seq<string * decimal * decimal>) = 
                x |> Seq.fold(fun (transactionTotal: decimal) (currencyCode, totalPerCurrencyAfterEchange, totalPerCurrencyBeforeExchange) ->
                transactionTotal + totalPerCurrencyAfterEchange) 0.0M
            
            let invoicingCurrencyTotalBeforeExchange = getTotalInvoiceAmount getTotalInvoiceAmountPerCurrency

            let invoicingCurrencyTotalAfterProfitMarginSplit = Math.getProfitMarginPercentageAsDecimal invoicingCurrencyTotalBeforeExchange profitMargin
                
            printf"%16s %O" selectedInvoicingCurrencyCode invoicingCurrencyTotalBeforeExchange

            printfn "\nCCS Profit - %s %O - EUR(%O)" selectedInvoicingCurrencyCode 
                                                    (invoicingCurrencyTotalAfterProfitMarginSplit) 
                                                    (Math.convertInvoicingCurrencyToEuro invoicingCurrencyTotalAfterProfitMarginSplit selectedInvoicingCurrencyCode)
     
        | false ->
            Console.Clear()  

            printfn "Please Wait ...... Generating Invoice"

module Sequence = 

    let containsCurrencyCode selectedInvoicingCurrencyCode seq = 
        Seq.exists (fun (currencyCode, _, _) -> currencyCode = selectedInvoicingCurrencyCode) seq

    let getTotalInvoiceAmount (x: seq<string * decimal * decimal>) = 
        x |> Seq.fold(fun (transactionTotal: decimal) (_, totalPerCurrencyAfterExchange, _) ->
        transactionTotal + totalPerCurrencyAfterExchange) 0.0M

module QueryDatabase =    

    let getAllTransactions (connection: NpgsqlConnection) (queryString: string) =
        let transactions = new List<Entity.Transcation>()
        let command = new NpgsqlCommand(queryString, connection)
        let dataReader = command.ExecuteReader()
        
        while dataReader.Read() do
            let transaction = {Entity.Transcation.MerchantId = dataReader.GetInt32(0); 
                               Entity.Transcation.MessageTypeId = dataReader.GetInt32(1); 
                               Entity.Transcation.SaleCurrencyId = dataReader.GetInt32(2);
                               Entity.Transcation.PaymentCurrencyId = dataReader.GetInt32(3);
                               Entity.Transcation.PaymentValue = dataReader.GetDouble(4);
                               Entity.Transcation.PaymentMarginValue = dataReader.GetDouble(5);
                               Entity.Transcation.CreationTimeStamp = dataReader.GetDateTime(6)}
            transactions.Add(transaction)
        transactions

    let getAllCurrenyCodes (connection: NpgsqlConnection) (queryString: string) =
        let currencyCodes = new List<Entity.Currency>()
        let command = new NpgsqlCommand(queryString, connection)
        let dataReader = command.ExecuteReader()
        
        while dataReader.Read() do
            let currencyCode = {Entity.Currency.Id = dataReader.GetInt32(0); 
                                Entity.Currency.Code = dataReader.GetString(1);}
            currencyCodes.Add(currencyCode)
        currencyCodes

    let insertInvoiceAsJsonIntoInvoiceTable (connection: NpgsqlConnection) (queryString: string) =
        let strippedQueryString = String.removeWhiteSpaceCarrageReturnAndNewLine queryString
        let command = new NpgsqlCommand(strippedQueryString, connection)
        
        let isInsertedIntoInvoiceTable x =
            match x with 
            | 1 -> printfn "Inserted Invoice Successfully!!"
            | _ -> printfn "!!Error inserting invoice"

        isInsertedIntoInvoiceTable (command.ExecuteNonQuery())
        
    let getAllTransaction (messageReceived : MessageType.InvoiceMessage) () =
        
        let conn = Database.openConnection
        conn.Open() 

        try
            let currencyCodes = getAllCurrenyCodes <| conn <| Database.getAllCurrenyCodesQueryString
            let transactions = getAllTransactions <| conn <| Database.getAllTransactionQueryString

            let selectedInvoicingCurrencyCode = Convert.invoiceCurrencyIdToCurrencyCode messageReceived.InvoiceCurrency

            let getTotalInvoiceAmountPerCurrency =
                query { for transaction in transactions do 
                        join currency in currencyCodes on
                                (transaction.SaleCurrencyId = currency.Id)
                        where (transaction.MessageTypeId = 9)
                        where (transaction.MerchantId = messageReceived.MerchantId)
                        where (transaction.CreationTimeStamp >= messageReceived.DateFrom)
                        where (transaction.CreationTimeStamp <= messageReceived.DateTo.AddHours(23.0).AddMinutes(59.0).AddSeconds(59.99))
                        groupBy currency.Code into getTotalInvoiceAmountPerCurrency
                        let sumBySaleCurrencyId =
                            query { for value in getTotalInvoiceAmountPerCurrency do
                                    let transaction, currrencyCode = value
                                    where (transaction.SaleCurrencyId <> Convert.invoiceCurrencyIdToCurrencyNum messageReceived.InvoiceCurrency)
                                    sumBy(decimal transaction.PaymentMarginValue)
                            }                                                               
                        select (getTotalInvoiceAmountPerCurrency.Key, 
                                Math.Round(sumBySaleCurrencyId / (Http.getExchangeRates getTotalInvoiceAmountPerCurrency.Key <| selectedInvoicingCurrencyCode), 2),
                                sumBySaleCurrencyId)
                }

            let numberOfRowsRequiredInExcelTable = if Sequence.containsCurrencyCode selectedInvoicingCurrencyCode getTotalInvoiceAmountPerCurrency then 
                                                       Seq.length getTotalInvoiceAmountPerCurrency - 1 
                                                   else Seq.length getTotalInvoiceAmountPerCurrency
            
            Excel.prepairRowInInvoiceTemplate numberOfRowsRequiredInExcelTable |> ignore

            let totalInvocieAmountInEuro = Http.getExchangeRates selectedInvoicingCurrencyCode "EUR"
            
            Calculate.displayGeneratedData getTotalInvoiceAmountPerCurrency selectedInvoicingCurrencyCode messageReceived.ProfitMargin totalInvocieAmountInEuro true ()
            
            let invoicingCurrencyTotalBeforeExchange = Sequence.getTotalInvoiceAmount getTotalInvoiceAmountPerCurrency

            let invoicingCurrencyTotalAfterProfitMarginSplit = Math.getProfitMarginPercentageAsDecimal invoicingCurrencyTotalBeforeExchange (messageReceived.ProfitMargin)

            let ccsProfitInEuro = (Math.convertInvoicingCurrencyToEuro invoicingCurrencyTotalAfterProfitMarginSplit selectedInvoicingCurrencyCode)

            let invoiceNumber = InvoiceNo.generate messageReceived.MerchantId

            Excel.generateInvoice getTotalInvoiceAmountPerCurrency messageReceived invoicingCurrencyTotalBeforeExchange invoicingCurrencyTotalAfterProfitMarginSplit 
                                  ccsProfitInEuro selectedInvoicingCurrencyCode numberOfRowsRequiredInExcelTable invoiceNumber

            let generatedJson = Json.generateJsonForInvoiceInDatabase getTotalInvoiceAmountPerCurrency messageReceived invoicingCurrencyTotalBeforeExchange invoicingCurrencyTotalAfterProfitMarginSplit 
                                                                      ccsProfitInEuro selectedInvoicingCurrencyCode numberOfRowsRequiredInExcelTable invoiceNumber

            let insertIntoInvoiceString = Database.insertIntoInvoiceString invoiceNumber generatedJson

            insertInvoiceAsJsonIntoInvoiceTable <| conn <| insertIntoInvoiceString

        finally
            conn.Close()

