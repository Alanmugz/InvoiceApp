
namespace InvoiceApp
 
open System
open System.IO
open System.Runtime.Serialization.Json
open System.Text

module Utilities = 
   
    let encode messageAsStr = 
      Encoding.ASCII.GetBytes(messageAsStr.ToString())

    let decode messageAsByte = 
      Encoding.ASCII.GetString(messageAsByte)

    let toString = System.Text.Encoding.ASCII.GetString
    let toBytes (_string : string) = System.Text.Encoding.ASCII.GetBytes _string

    let serializeJson<'a> (x : 'a) = 
        let jsonSerializer = new DataContractJsonSerializer(typedefof<'a>)
 
        use stream = new MemoryStream()
        jsonSerializer.WriteObject(stream, x)
        toString <| stream.ToArray()
