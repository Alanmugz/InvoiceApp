
namespace InvoiceApp
 
open System
open System.IO
open System.Runtime.Serialization.Json
open System.Text

module Utilities = 

    type InvoiceMessage = {DateFrom: DateTime; DateTo: DateTime; InvoiceCurrency: int32; MerchantId: int32; ProfitMargin: double}

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

    let connectionString = "Server = localhost; Port = 5432; Database = InvoiceApplication; User Id = postgres; Password = y6j5atu5 ; CommandTimeout = 40;"
