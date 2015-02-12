
namespace InvoiceApp
 
open DatabaseConnection
open fszmq
open fszmq.Context
open fszmq.Socket
open System
open Utilities


module ZeroMQServer =

    let server () =
        // create a ZMQ context
        use context = new Context()
  
        // create reply socket
        use server  = rep context
        // begin receiving connections
        bind server "tcp://*:5555"

        printf "Starting server ....... started!!!!!\n"
  
        while true do
            // process request (i.e. 'recv' a message from our 'server')
            // NOTE: it's convenient to 'decode' the (binary) message into a string
            let recievedMessage = server |> recv |> decode |> deserializeJson<InvoiceMessage>

            getAll recievedMessage ()

            "Recieved"B |> Socket.send server
