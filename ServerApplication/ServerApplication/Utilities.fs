﻿
namespace InvoiceApp
 
open System
open System.IO
open System.Runtime.Serialization.Json
open System.Text

module Utilities = 

    module MessageTypes = 
        type InvoiceMessage = {DateFrom: DateTime; DateTo: DateTime; InvoiceCurrency: int32; MerchantId: int32; ProfitMargin: decimal}

    module RelationTable = 
        [<CLIMutable>]
        type Transcation = {MerchantId: int; 
                            MessageTypeId: int; 
                            SaleCurrencyId: int;
                            PaymentCurrencyId: int;
                            PaymentValue: double;
                            PaymentMarginValue: double;
                            CreationTimeStamp: DateTime;}

        [<CLIMutable>]
        type Currencycode = {CurrencyId: int; 
                         CurrencyCode: string;}

    module ZeroMQ = 
        let encode messageAsStr = 
          Encoding.ASCII.GetBytes(messageAsStr.ToString())

        let decode messageAsByte = 
          Encoding.ASCII.GetString(messageAsByte)

        let toString = System.Text.Encoding.ASCII.GetString
        let toBytes (string : string) = System.Text.Encoding.ASCII.GetBytes string

        let deserializeJson<'a> (json : string) =
            let jsonSerializer = new DataContractJsonSerializer(typedefof<'a>)
 
            use stream = new MemoryStream(toBytes json)
            jsonSerializer.ReadObject(stream) :?> 'a

        let serializeJson<'a> (x : 'a) = 
            let jsonSerializer = new DataContractJsonSerializer(typedefof<'a>)
 
            use stream = new MemoryStream()
            jsonSerializer.WriteObject(stream, x)
            toString <| stream.ToArray()

    module Database = 
        let private connectionString = "Server = localhost; Port = 5432; Database = InvoiceApplication; User Id = postgres; Password = y6j5atu5 ; CommandTimeout = 40;"

        let openConnection = new Npgsql.NpgsqlConnection(connectionString)

        let getAllTransactionQueryString = "SELECT \"MerchantId\", \"MessageTypeId\", \"SaleCurrencyId\", \"PaymentCurrencyId\", \"PaymentValue\", \"PaymentMarginValue\", \"CreationTimestamp\" 
                                            FROM \"Transaction\""

        let getAllCurrenyCodesQueryString = "SELECT * 
                                             FROM \"SaleCurrrency\""


    let getCurrencyCodeStr currency = 
        match currency with 
        | 1 -> "USD"
        | 2 -> "EUR"
        | 3 -> "CAD"
        | 4 -> "GBP"
        | 5 -> "NZD"
        | 6 -> "JPY"
        | _ -> ""

    let getCurrencyCodeInt currency = 
        match currency with 
        | 1 -> 840
        | 2 -> 978
        | 3 -> 124
        | 4 -> 826
        | 5 -> 554
        | 6 -> 392
        | _ -> 0