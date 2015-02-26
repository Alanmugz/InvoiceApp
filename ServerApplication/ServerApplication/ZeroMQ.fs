
namespace InvoiceApp
 
open fszmq
open fszmq.Context
open fszmq.Socket
open System
open System.Threading

module ZeroMQ =

    let startServer () =
        // create a ZMQ context
        use context = new Context()
        // create reply socket
        use server  = rep context
        // open receiving connections
        bind server "tcp://*:5555"

        printf "Starting server ....... started!!!!!\n"

        let rec listenForMessage () =
            let messageReceived = server |> recv |> ZeroMQHelper.decode |> ZeroMQHelper.deserializeJson<MessageType.InvoiceMessage>

            QueryDatabase.getAllTransaction messageReceived ()
 
            "Invoice Generated Succesfully"B |> Socket.send server
        listenForMessage ()
        listenForMessage ()

