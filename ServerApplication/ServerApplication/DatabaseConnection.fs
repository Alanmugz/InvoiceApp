
namespace InvoiceApp
 
open Npgsql
open System
open System.Collections.Generic

module DatabaseConnection =

    [<CLIMutable>]
    type Transcation = {MerchantId: int; 
                        MessageTypeId: int; 
                        SaleCurrencyId: int;
                        PaymentCurrencyId: int;
                        PaymentValue: double;
                        PaymentMarginValue: double;
                        CreationTimeStamp: DateTime;}


    let private connectionString = "Server = localhost; Port = 5432; Database = InvoiceApplication; User Id = postgres; Password = y6j5atu5 ; CommandTimeout = 40;"
    
    let private getResultSet (connection:NpgsqlConnection) (queryString:string) =
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
        

    let printResults (message : Utilities.InvoiceMessage) () =
        
        let conn = new NpgsqlConnection(connectionString)
        conn.Open()

        let query = String.Format("SELECT \"MerchantId\", \"MessageTypeId\", \"SaleCurrencyId\", \"PaymentCurrencyId\", \"PaymentValue\", \"PaymentMarginValue\", \"CreationTimestamp\" 
                                  FROM \"Transaction\" 
                                  WHERE \"MerchantId\" = {0} AND \"CreationTimestamp\"  >= '{1} 00:00:00'::timestamp without time zone 
                                                           AND \"CreationTimestamp\"  <= '{2} 23:59:59'::timestamp without time zone
                                  ORDER BY \"CreationTimestamp\";", message.MerchantId, message.DateFrom.ToShortDateString(), message.DateTo.ToShortDateString())

        try
            let resultSet = getResultSet <| conn <| query

            for value in resultSet do
                printfn "%A" (value.ToString())
        finally
            conn.Close()

