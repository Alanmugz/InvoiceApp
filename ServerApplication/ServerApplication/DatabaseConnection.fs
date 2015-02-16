
namespace InvoiceApp

open HttpRates
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
        
    let getAllTransactions (_message : Utilities.MessageTypes.InvoiceMessage) () =
        
        let conn = Utilities.Database.openConnection
        conn.Open()

        try
            let resultSetCurrencyCode = getAllCurrenyCodesResultsSet <| conn <| Utilities.Database.getAllCurrenyCodesQueryString
            let resultSetTransaction = getAllTransactionResultSet <| conn <| Utilities.Database.getAllTransactionQueryString

            let selectedCurrencyCode = Utilities.getCurrencyCode _message.InvoiceCurrency

            (* rsc - resultSetCurrencyCode
               rst - resultSetTransaction *)

            let linqQuery =
                query { for rst in resultSetTransaction do 
                        join rct in resultSetCurrencyCode on
                              (rst.SaleCurrencyId = rct.CurrencyId)
                        where (rst.MerchantId = _message.MerchantId)
                        where (rst.CreationTimeStamp >= _message.DateFrom)
                        where (rst.CreationTimeStamp <= _message.DateTo.AddHours(23.0).AddMinutes(59.0).AddSeconds(59.9))
                        groupBy rct.CurrencyCode into group
                        let sumByGroup =
                            query { for value in group do
                                    let transaction, currrencyCode = value
                                    sumBy(decimal transaction.PaymentMarginValue)
                            }                                                               
                        select (group.Key, Math.Round(sumByGroup / (getRates group.Key <| selectedCurrencyCode), 2))
                }
                 
            Console.Clear()

            linqQuery |> Seq.iter (fun (currencyCode, totalPerCurrency) -> printf "%s - %s %O\n" currencyCode (selectedCurrencyCode) totalPerCurrency)
            printf "-------------------\n" 

            let sumAllGroupedTransactions (x: seq<string * decimal>) = 
                x |> Seq.fold(fun (transactionTotal: decimal) (currencyCode, totalPerCurrency) -> transactionTotal + totalPerCurrency) 0.0M
            
            let total = sumAllGroupedTransactions linqQuery
                
            printf"      %s %A" (selectedCurrencyCode) total

            printfn "\nCCS Profit - %s %A" (selectedCurrencyCode) (Math.Round((total / 100.0M * _message.ProfitMargin),2))

            printInvoice linqQuery

        finally
            conn.Close()

