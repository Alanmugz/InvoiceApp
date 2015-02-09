
namespace InvoiceApp
 
open fszmq
open fszmq.Context
open fszmq.Socket
open System

module ZeroMQ = 

    let client () =
    // create a ZMQ context
        use context = new Context()
  
        // create a request socket
        use client  = req context
        // connect to the server
        "tcp://localhost:5555" |> connect client

        for i in 1 .. 1 do
          // 'send' a request to the server
          let request = "hello"
          // NOTE: we need to 'encode' a string to binary (before transmission)
          request |> Utilities.encode |> send client
          printfn "(%i) sent: %s" i request
          // receive and print a reply from the server
          let reply = (recv >> Utilities.decode) client
          printfn "(%i) got: %s" i reply


