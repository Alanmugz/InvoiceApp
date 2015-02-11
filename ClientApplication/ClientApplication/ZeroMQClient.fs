
namespace InvoiceApp
 
open fszmq
open fszmq.Context
open fszmq.Socket
open System
open Utilities

module ZeroMQClient =

    let client m_userInpuTuple =
        let dateFrom, dateTo, invoiceCurrency, merchantId, profitMargin = m_userInpuTuple
        let message = {DateFrom = dateFrom; DateTo = dateTo; InvoiceCurrency = invoiceCurrency; MerchantId = merchantId; ProfitMargin = profitMargin}

        // create a ZMQ context
        use context = new Context()
  
        // create a request socket
        use client  = req context
        // connect to the server
        "tcp://localhost:5555" |> connect client

        for i in 1 .. 1 do
            // 'send' a request to the server
            printf "%A" (m_userInpuTuple.GetType())
            let message = encode <| serializeJson<InvoiceMessage> message
            printfn "Sending Message......"

            //Sends message to server
            message |> Socket.send client
            printfn "Message sent!!"

            //Recieves message as byte array a decode to string
            let messageAsString = client |> Socket.recv |> decode
            printfn "Reply: %A" messageAsString


