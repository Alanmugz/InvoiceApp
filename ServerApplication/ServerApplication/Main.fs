
namespace InvoiceApp
 
open System

module Main =
    
    let test = new TestClass()
    test.RunAll true ()

    [<EntryPoint>]
    ZeroMQ.startServer ()