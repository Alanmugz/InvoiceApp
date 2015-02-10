
namespace InvoiceApp
 
open Npgsql
open System

module DatabaseConnection =

    let private connectionString = "Server = localhost; Port = 5432; Database = InvoiceApplication; User Id = postgres; Password = y6j5atu5 ; CommandTimeout = 40;"
    let private getResultSet (connection:NpgsqlConnection) (queryString:string) =
        let command = new NpgsqlCommand(queryString, connection)
        let dataReader = command.ExecuteReader()
        seq {
            while dataReader.Read() do
                yield [for i in [0..dataReader.FieldCount-1] -> dataReader.[i]]
        }

    let printResults (message : Utilities.InvoiceMessage) () =
        let conn = new NpgsqlConnection(connectionString)
        conn.Open()

        let query = String.Format("SELECT \"TransactionId\" 
                                  FROM \"Transaction\" 
                                  WHERE \"MerchantId\" = {0} AND \"CreationTimestamp\"  >= '{1} 00:00:00'::timestamp without time zone 
                                                           AND \"CreationTimestamp\"  <= '{2} 23:59:59'::timestamp without time zone
                                  ORDER BY \"CreationTimestamp\" Limit 10;", message.MerchantId, message.DateFrom.ToShortDateString(), message.DateTo.ToShortDateString())

        try
            let resultSet = getResultSet <| conn <| query
            for r in resultSet do
                for d in r do
                    printf "%s\t" (d.ToString())
                printfn ""
        finally
            conn.Close()

