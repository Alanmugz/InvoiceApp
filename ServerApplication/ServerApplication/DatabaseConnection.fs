
namespace InvoiceApp

open Npgsql
open System
open System.Collections.Generic
open System.Linq
open Utilities
open Excel

module QueryDatabase =
    
    let private getAllTransactionResultSet (connection: NpgsqlConnection) (queryString: string) =
        let transactionLists = new List<Utilities.RelationTable.Transcation>()
        let command = new NpgsqlCommand(queryString, connection)
        let dataReader = command.ExecuteReader()
        
        while dataReader.Read() do
            let transaction = {Utilities.RelationTable.Transcation.MerchantId = dataReader.GetInt32(0); 
                               Utilities.RelationTable.Transcation.MessageTypeId = dataReader.GetInt32(1); 
                               Utilities.RelationTable.Transcation.SaleCurrencyId = dataReader.GetInt32(2);
                               Utilities.RelationTable.Transcation.PaymentCurrencyId = dataReader.GetInt32(3);
                               Utilities.RelationTable.Transcation.PaymentValue = dataReader.GetDouble(4);
                               Utilities.RelationTable.Transcation.PaymentMarginValue = dataReader.GetDouble(5);
                               Utilities.RelationTable.Transcation.CreationTimeStamp = dataReader.GetDateTime(6)}
            transactionLists.Add(transaction)
        transactionLists

    let private getAllCurrenyCodesResultsSet (connection: NpgsqlConnection) (queryString: string) =
        let currencyCodeList = new List<Utilities.RelationTable.Currencycode>()
        let command = new NpgsqlCommand(queryString, connection)
        let dataReader = command.ExecuteReader()
        
        while dataReader.Read() do
            let currencyCode = {Utilities.RelationTable.Currencycode.CurrencyId = dataReader.GetInt32(0); 
                                Utilities.RelationTable.Currencycode.CurrencyCode = dataReader.GetString(1);}
            currencyCodeList.Add(currencyCode)
        currencyCodeList
        
    let getAllTransactions (message : Utilities.MessageTypes.InvoiceMessage) () =
        
        let conn = Utilities.Database.openConnection
        conn.Open()

        try
            let resultSetCurrencyCode = getAllCurrenyCodesResultsSet <| conn <| Utilities.Database.getAllCurrenyCodesQueryString
            let resultSetTransaction = getAllTransactionResultSet <| conn <| Utilities.Database.getAllTransactionQueryString

            let selectedInvoicingCurrencyCode = Utilities.getCurrencyCodeStr message.InvoiceCurrency

            (* rsc - resultSetCurrencyCode
               rst - resultSetTransaction *)

            let getTotalInvoiceAmountPerCurrency =
                query { for rst in resultSetTransaction do 
                        join rct in resultSetCurrencyCode on
                              (rst.SaleCurrencyId = rct.CurrencyId)
                        where (rst.MessageTypeId = 9)
                        where (rst.MerchantId = message.MerchantId)
                        where (rst.CreationTimeStamp >= message.DateFrom)
                        where (rst.CreationTimeStamp <= message.DateTo.AddHours(23.0).AddMinutes(59.0).AddSeconds(59.99))
                        groupBy rct.CurrencyCode into getTotalInvoiceAmountPerCurrency
                        let sumByGroup =
                            query { for value in getTotalInvoiceAmountPerCurrency do
                                    let transaction, currrencyCode = value
                                    where (transaction.SaleCurrencyId <> Utilities.getCurrencyCodeInt message.InvoiceCurrency)
                                    sumBy(decimal transaction.PaymentMarginValue)
                            }                                                               
                        select (getTotalInvoiceAmountPerCurrency.Key, Math.Round(sumByGroup / (Http.getRates getTotalInvoiceAmountPerCurrency.Key <| selectedInvoicingCurrencyCode), 2), sumByGroup)
                }
                 
            Console.Clear()

            getTotalInvoiceAmountPerCurrency |> Seq.iter (fun (currencyCode, totalPerCurrencyAfterEchange, totalPerCurrencyBeforeExchange) -> printf "%s - %s %O\n" currencyCode selectedInvoicingCurrencyCode totalPerCurrencyAfterEchange)
            printf "-------------------\n" 

            let finalInvoiceAmount (x: seq<string * decimal * decimal>) = 
                x |> Seq.fold(fun (transactionTotal: decimal) (currencyCode, totalPerCurrencyAfterEchange, totalPerCurrencyBeforeExchange) -> transactionTotal + totalPerCurrencyAfterEchange) 0.0M
            
            let total = finalInvoiceAmount getTotalInvoiceAmountPerCurrency
                
            printf"     %s %A" selectedInvoicingCurrencyCode total

            printfn "\nCCS Profit - %s %A" selectedInvoicingCurrencyCode (Math.Round((total / 100.0M * message.ProfitMargin),2))

            printInvoice getTotalInvoiceAmountPerCurrency message

        finally
            conn.Close()

