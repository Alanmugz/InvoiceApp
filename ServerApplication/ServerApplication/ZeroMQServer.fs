
namespace InvoiceApp
 
open fszmq
open fszmq.Context
open fszmq.Socket
open System

type InvoiceMessage = {DateFrom: DateTime; DateTo: DateTime; InvoiceCurrency: int32; MerchantId: int32; ProfitMargin: double}

module ZeroMQServer =

    let server () =
      // create a ZMQ context
      use context = new Context()
  
      // create reply socket
      use server  = rep context
      // begin receiving connections
      bind server "tcp://*:5555"
  
      let rec loop () =
        // process request (i.e. 'recv' a message from our 'server')
        // NOTE: it's convenient to 'decode' the (binary) message into a string
        let messageReceived = server |> Socket.recv |> Utilities.decode |> Utilities.deserializeJson<InvoiceMessage>

        printfn "Message recieved is %A" messageReceived

      // wait for next request
      loop () 
