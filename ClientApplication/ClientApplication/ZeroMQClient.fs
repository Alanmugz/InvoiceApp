
namespace InvoiceApp
 
open fszmq
open fszmq.Context
open fszmq.Socket
open System

module ZeroMQClient =
    
    type InvoiceMessage = {DateFrom: DateTime; DateTo: DateTime; InvoiceCurrency: int32; MerchantId: int32; ProfitMargin: double}

    let client userInpuTuple =
        let dateFrom, dateTo, invoiceCurrency, merchantId, profitMargin = userInpuTuple
        let a = {DateFrom = dateFrom; DateTo = dateTo; InvoiceCurrency = invoiceCurrency; MerchantId = merchantId; ProfitMargin = profitMargin}

        // create a ZMQ context
        use context = new Context()
  
        // create a request socket
        use client  = req context
        // connect to the server
        "tcp://localhost:5555" |> connect client

        for i in 1 .. 1 do
            // 'send' a request to the server
            let message = Utilities.encode <| Utilities.serializeJson<InvoiceMessage> a
            printf "%A" message
            printfn "Sending Message....."

            //Sends message to server
            message |> Socket.send client
            printfn "Message sent!!"

            // receive and print a reply from the server
            let reply = (recv >> Utilities.decode) client
            printfn "(%i) got: %s" i reply


