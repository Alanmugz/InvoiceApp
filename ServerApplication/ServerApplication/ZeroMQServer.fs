
namespace InvoiceApp
 
open fszmq
open fszmq.Context
open fszmq.Socket
open System
open System.Threading


module ZeroMQServer =

    let server () =
        // create a ZMQ context
        use context = new Context()
  
        // create reply socket
        use server  = rep context
        // open receiving connections
        bind server "tcp://*:5555"

        printf "Starting server ....... started!!!!!\n"

        let rec listenForMessage () =
            let recievedMessage = server |> recv |> Utilities.ZeroMQ.decode |> Utilities.ZeroMQ.deserializeJson<Utilities.MessageTypes.InvoiceMessage>

            QueryDatabase.getAllTransactions recievedMessage ()

            //Reply 
            "Recieved"B |> Socket.send server
        listenForMessage ()

        listenForMessage ()
