
namespace InvoiceApp
 
open System

module main =
   
  [<EntryPoint>]
  ZeroMQServer.server()
   
  Console.ReadKey() |> ignore