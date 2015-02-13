
namespace InvoiceApp

open HttpRates
open Npgsql
open System
open System.Collections.Generic
open System.Linq
open Utilities

module DatabaseConnection =
    
    let private getAllTransactionResultSet (connection: NpgsqlConnection) (queryString: string) =
        let transactionLists = new List<Transcation>()
        let command = new NpgsqlCommand(queryString, connection)
        let dataReader = command.ExecuteReader()
        
        while dataReader.Read() do
            let transaction = {MerchantId = dataReader.GetInt32(0); 
                               MessageTypeId = dataReader.GetInt32(1); 
                               SaleCurrencyId = dataReader.GetInt32(2);
                               PaymentCurrencyId = dataReader.GetInt32(3);
                               PaymentValue = dataReader.GetDouble(4);
                               PaymentMarginValue = dataReader.GetDouble(5);
                               CreationTimeStamp = dataReader.GetDateTime(6)}
            transactionLists.Add(transaction)
        transactionLists

    let private getAllCurrenyCodesResultsSet (connection: NpgsqlConnection) (queryString: string) =
        let currencyCodeList = new List<Currencycode>()
        let command = new NpgsqlCommand(queryString, connection)
        let dataReader = command.ExecuteReader()
        
        while dataReader.Read() do
            let currencyCode = {CurrencyId = dataReader.GetInt32(0); 
                                CurrencyCode = dataReader.GetString(1);}
            currencyCodeList.Add(currencyCode)
        currencyCodeList
        
    let getAll (_message : InvoiceMessage) () =
        
        let conn = new NpgsqlConnection(connectionString)
        conn.Open()

        let getAllTransactionQueryString = String.Format("SELECT \"MerchantId\", \"MessageTypeId\", \"SaleCurrencyId\", \"PaymentCurrencyId\", \"PaymentValue\", \"PaymentMarginValue\", \"CreationTimestamp\" 
                                                          FROM \"Transaction\"")

        let getAllCurrenyCodesQueryString = String.Format("SELECT * 
                                                           FROM \"SaleCurrrency\"")

        try
            let resultSetCurrencyCode = getAllCurrenyCodesResultsSet <| conn <| getAllCurrenyCodesQueryString
            let resultSetTransaction = getAllTransactionResultSet <| conn <| getAllTransactionQueryString

            let linqExample =
                query { for rst in resultSetTransaction do 
                        join rct in resultSetCurrencyCode on
                              (rst.SaleCurrencyId = rct.CurrencyId)
                        where (rst.MerchantId = _message.MerchantId)
                        where (rst.CreationTimeStamp >= _message.DateFrom)
                        where (rst.CreationTimeStamp <= _message.DateTo.AddHours(23.0).AddMinutes(59.0).AddSeconds(59.9))
                        groupBy rct.CurrencyCode into g
                        let sum =
                            query { for p in g do
                                    let a, b = p
                                    sumBy(decimal a.PaymentMarginValue)
                            }
                        select (g.Key, Math.Round(sum / (getRates g.Key <| getCurrency _message.MerchantId), 2))
                } 
            Console.Clear()
            linqExample |> Seq.iter (fun (x, y) -> printf "%s - %s %O\n" x (getCurrency _message.InvoiceCurrency) y)
            printf "-------------------\n" 

            let sum (x: seq<string * decimal>) = x |> Seq.fold(fun (acc: decimal) (a, b) -> acc + b) 0.0M
            
            let total = sum linqExample
                
            printf"      %s %A" (getCurrency _message.InvoiceCurrency) total

            printfn "\nCCS Profit - %s %A" (getCurrency _message.InvoiceCurrency) (Math.Round((total / 100.0M * 2.3M),2))

        finally
            conn.Close()

