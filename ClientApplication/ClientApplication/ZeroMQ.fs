
namespace InvoiceApp
 
open fszmq
open System

module ZeroMQ = 

    let client () =
        use context = new Context()

        // socket to talk to server
        printfn "Connecting to Server...........Connected"
        use requester = context |> Context.req
        "tcp://localhost:5555" |> Socket.connect requester

        while true do
            let message = Z85.decode "hello"
            printfn "Sending Message....."

            //Sends message to server
            message |> Socket.send requester
            printfn "Message sent!!"

            //Recieves message as byte array a decode to string
            let messageAsString = requester |> Socket.recv |> Z85.encode
            printfn "Received: %A" messageAsString
