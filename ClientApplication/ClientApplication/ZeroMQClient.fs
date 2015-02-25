
namespace InvoiceApp
 
open fszmq
open fszmq.Context
open fszmq.Socket
open System

module ZeroMQClient =

    let client userInputTuple =
        let dateFrom, dateTo, invoiceCurrency, merchantId, profitMargin, x = userInputTuple

        let messageReceived = {MessageType.InvoiceMessage.DateFrom = dateFrom; 
                               MessageType.InvoiceMessage.DateTo = dateTo; 
                               MessageType.InvoiceMessage.InvoiceCurrency = invoiceCurrency; 
                               MessageType.InvoiceMessage.MerchantId = merchantId; 
                               MessageType.InvoiceMessage.ProfitMargin = profitMargin;
                               MessageType.InvoiceMessage.InvoiceNumber = x }

        // create a ZMQ context
        use context = new Context()
  
        // create a request socket
        use client  = req context
        // connect to the server
        "tcp://localhost:5555" |> connect client

        let sendMessageWaitForReply () = 
            // 'send' a request to the server
            let messageToBeSent = ZeroMQHelper.encode <| ZeroMQHelper.serializeJson<MessageType.InvoiceMessage> messageReceived
            printfn "Sending Message......"

            //Sends message to server
            messageToBeSent |> Socket.send client
            printfn "Message sent!!"

            //Recieves message as byte array a decode to string
            let replyMessage = client |> Socket.recv |> ZeroMQHelper.decode
            printfn "Reply: %A" replyMessage

        sendMessageWaitForReply ()
