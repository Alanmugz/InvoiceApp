
namespace InvoiceApp
 
open fszmq
open fszmq.Context
open fszmq.Socket
open System

module ZeroMQClient =

    let client m_userInputTuple =
        let dateFrom, dateTo, invoiceCurrency, merchantId, profitMargin = m_userInputTuple
        let message = {MessageType.InvoiceMessage.DateFrom = dateFrom; MessageType.InvoiceMessage.DateTo = dateTo; MessageType.InvoiceMessage.InvoiceCurrency = invoiceCurrency; MessageType.InvoiceMessage.MerchantId = merchantId; MessageType.InvoiceMessage.ProfitMargin = profitMargin}

        // create a ZMQ context
        use context = new Context()
  
        // create a request socket
        use client  = req context
        // connect to the server
        "tcp://localhost:5555" |> connect client

        let sendMessageWaitForReply () = 
            // 'send' a request to the server
            let message = ZeroMQHelper.encode <| ZeroMQHelper.serializeJson<MessageType.InvoiceMessage> message
            printfn "Sending Message......"

            //Sends message to server
            message |> Socket.send client
            printfn "Message sent!!"

            //Recieves message as byte array a decode to string
            let messageAsString = client |> Socket.recv |> ZeroMQHelper.decode
            printfn "Reply: %A" messageAsString

        sendMessageWaitForReply ()
