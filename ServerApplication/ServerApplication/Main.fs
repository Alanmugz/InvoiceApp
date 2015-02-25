
namespace InvoiceApp
 
open System

module Main =
    
    let test = new TestClass()
    test.RunAll false ()

    [<EntryPoint>]
    ZeroMQ.startServer ()