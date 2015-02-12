
namespace InvoiceApp

open Npgsql
open System
open System.Collections.Generic
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
                                CurrencyCode = dataReader.GetInt32(1);}
            currencyCodeList.Add(currencyCode)
        currencyCodeList

        
    let getAll (_message : InvoiceMessage) () =
        
        let conn = new NpgsqlConnection(connectionString)
        conn.Open()

        let getAllTransactionQueryString = String.Format("SELECT \"MerchantId\", \"MessageTypeId\", \"SaleCurrencyId\", \"PaymentCurrencyId\", \"PaymentValue\", \"PaymentMarginValue\", \"CreationTimestamp\" 
                                                          FROM \"Transaction\"")

        let getAllCurrenyCodes = String.Format("SELECT * 
                                                FROM \"SaleCurrency\"")

        try
            let resultSetCurrencyCode = getAllCurrenyCodesResultsSet <| conn <| getAllTransactionQueryString
            let resultSetTransaction = getAllTransactionResultSet <| conn <| getAllTransactionQueryString

            let linqExample =
                query { for rst in resultSetTransaction do
                        where (rst.MerchantId = _message.MerchantId)
                        where (rst.CreationTimeStamp >= _message.DateFrom)
                        where (rst.CreationTimeStamp <= _message.DateTo.AddHours(23.0).AddMinutes(59.0).AddSeconds(59.9))
                        sortBy rst.SaleCurrencyId
                        groupBy rst.SaleCurrencyId into group
                        let sum =
                            query { for p in group do
                                    sumBy(double p.PaymentMarginValue)
                            }
                        select (group.Key, sum)
                } |> Seq.toList
            Console.Clear()
            linqExample |> Seq.iter (fun x -> printf "%A\n" x)
        finally
            conn.Close()

