
namespace InvoiceApp
 
open NUnit.Framework
open System

[<TestFixture>]
type TestClass() =

    let testPassed testName = printfn "Test Passed!! - %s" testName
    let testFailed testName = printfn "Test Failed!! - %s" testName
    
    [<Test>]
    member this.``Get Profit Margin Percentage As Decimal Test``() =
        try
            Assert.AreEqual(20.00M, Math.getProfitMarginPercentageAsDecimal 200.00M 10.00M)
            Assert.AreEqual(11.67M, Math.getProfitMarginPercentageAsDecimal 507.56M 2.3M)
            Assert.AreEqual(235.89M, Math.getProfitMarginPercentageAsDecimal 10256.00M 2.3M)

            testPassed "Get Profit Margin Percentage As Decimal Test"
        with
        | :? NUnit.Framework.AssertionException as ex -> 
            testFailed "Get Profit Margin Percentage As Decimal Test"
            printfn "%s" ex.Message

    [<Test>]
    member this.``Convert Invoicing Currency To Euro Test``() =
        try
            let result1 = Http.getExchangeRates "EUR" "USD"

            Assert.That(result1 * 200.00M, Is.EqualTo(Math.convertInvoicingCurrencyToEuro 200.00M "USD").Within(0.01))

            testPassed "Convert Invoicing Currency To Euro Test"
        with
        | :? NUnit.Framework.AssertionException as ex -> 
            testFailed "Convert Invoicing Currency To Euro Test"
            printfn "%s" ex.Message

    [<Test>]
    member this.``Invoice CurrencyId To CurrencyNumber Test``() =
        try
            Assert.AreEqual(840, Convert.invoiceCurrencyIdToCurrencyNum 1)
            Assert.AreEqual(978, Convert.invoiceCurrencyIdToCurrencyNum 2)
            Assert.AreEqual(124, Convert.invoiceCurrencyIdToCurrencyNum 3)
            Assert.AreEqual(826, Convert.invoiceCurrencyIdToCurrencyNum 4)
            Assert.AreEqual(554, Convert.invoiceCurrencyIdToCurrencyNum 5)
            Assert.AreEqual(392, Convert.invoiceCurrencyIdToCurrencyNum 6)
            Assert.AreEqual(0, Convert.invoiceCurrencyIdToCurrencyNum 7)
            Assert.AreEqual(0, Convert.invoiceCurrencyIdToCurrencyNum 125)

            testPassed "Invoice CurrencyId To CurrencyNumber Test"
        with
        | :? NUnit.Framework.AssertionException as ex -> 
            testFailed "Invoice CurrencyId To CurrencyNumber Test"
            printfn "%s" ex.Message

    [<Test>]
    member this.``Invoice CurrencyId To CurrencyCode Test``() =
        try
            Assert.AreEqual("USD", Convert.invoiceCurrencyIdToCurrencyCode 1)
            Assert.AreEqual("EUR", Convert.invoiceCurrencyIdToCurrencyCode 2)
            Assert.AreEqual("CAD", Convert.invoiceCurrencyIdToCurrencyCode 3)
            Assert.AreEqual("GBP", Convert.invoiceCurrencyIdToCurrencyCode 4)
            Assert.AreEqual("NZD", Convert.invoiceCurrencyIdToCurrencyCode 5)
            Assert.AreEqual("JPY", Convert.invoiceCurrencyIdToCurrencyCode 6)
            Assert.AreEqual("", Convert.invoiceCurrencyIdToCurrencyCode 45)
            Assert.AreEqual("", Convert.invoiceCurrencyIdToCurrencyCode 8125)

            testPassed "Invoice CurrencyId To CurrencyCode Test"
        with
        | :? NUnit.Framework.AssertionException as ex -> 
            testFailed "Invoice CurrencyId To CurrencyCode Test"
            printfn "%s" ex.Message

    [<Test>]
    member this.``Encode String To Byte[] Test``() =
        try
            Assert.AreEqual("Hello World"B, ZeroMQHelper.encode "Hello World")
            Assert.AreNotEqual("Some Text"B, ZeroMQHelper.encode "Random String")

            testPassed "Encode String To Byte[] Test"
        with
        | :? NUnit.Framework.AssertionException as ex -> 
            testFailed "Encode String To Byte[] Test"
            printfn "%s" ex.Message

    [<Test>]
    member this.``Decode Byte[] To String Test``() =
        try
            Assert.AreEqual("Hello World", ZeroMQHelper.decode "Hello World"B)
            Assert.AreNotEqual("Some Text", ZeroMQHelper.decode "Random String of text"B)

            testPassed "Decode Byte[] To String Test"
        with
        | :? NUnit.Framework.AssertionException as ex -> 
            testFailed "Decode Byte[] To String Test"
            printfn "%s" ex.Message

    [<Test>]
    member this.``SerializeJson Test``() =
        try
            
            let message = {MessageType.InvoiceMessage.DateFrom = System.DateTime.Parse "20-01-2015 00:00:00"; MessageType.InvoiceMessage.DateTo = System.DateTime.Parse "21-01-2015 00:00:00"; 
                           MessageType.InvoiceMessage.InvoiceCurrency = 1; MessageType.InvoiceMessage.MerchantId = 1; 
                           MessageType.InvoiceMessage.ProfitMargin = 2.3M}

            let expected = "{\"DateFrom@\":\"\\/Date(1421712000000+0000)\\/\",\"DateTo@\":\"\\/Date(1421798400000+0000)\\/\",\"InvoiceCurrency@\":1,\"MerchantId@\":1,\"ProfitMargin@\":2.3}"

            Assert.AreEqual(expected, ZeroMQHelper.serializeJson<MessageType.InvoiceMessage> message)

            testPassed "SerializeJson Test"
        with
        | :? NUnit.Framework.AssertionException as ex -> 
            testFailed "SerializeJson Test"
            printfn "%s" ex.Message

    [<Test>]
    member this.``DeserializeJson Test``() =
        try
            
            let json = "{\"DateFrom@\":\"\\/Date(1421712000000+0000)\\/\",\"DateTo@\":\"\\/Date(1421798400000+0000)\\/\",\"InvoiceCurrency@\":1,\"MerchantId@\":1,\"ProfitMargin@\":2.3}"

            let deserializeJson = ZeroMQHelper.deserializeJson<MessageType.InvoiceMessage> json

            Assert.AreEqual(System.DateTime.Parse "20-01-2015 00:00:00", deserializeJson.DateFrom)
            Assert.AreEqual(System.DateTime.Parse "21-01-2015 00:00:00", deserializeJson.DateTo)
            Assert.AreEqual(1, deserializeJson.InvoiceCurrency)
            Assert.AreEqual(1, deserializeJson.MerchantId)
            Assert.AreEqual(2.3M, deserializeJson.ProfitMargin)

            testPassed "DeserializeJson Test"
        with
        | :? NUnit.Framework.AssertionException as ex -> 
            testFailed "DeserializeJson Test"
            printfn "%s" ex.Message

    [<Test>]
    member this.``Get Exchange Rates Test``() =
        try
            Assert.AreEqual(1.00000M, Http.getExchangeRates "EUR" "EUR")            

            testPassed "Get Exchange Rates Test"
        with
        | :? NUnit.Framework.AssertionException as ex -> 
            testFailed "Get Exchange Rates Test"
            printfn "%s" ex.Message

    [<Test>]
    member this.``Open Database Connection Test``() =
        try
            let conn = Database.openConnection  
            try
                conn.Open()  
            
                finally
                conn.Close()   

            testPassed "Open Database Connection Test"
        with
        | :? Npgsql.NpgsqlException as ex -> 
            testFailed "Open Database Connection Test"
            printfn "%s" ex.Message

    [<Test>]
    member this.``Sequence Contains CurrencyCode Test``() =
        try
            let seq = seq{ yield ("USD","",""); 
                           yield ("EUR","","") }

            let result1 = Sequence.containsCurrencyCode "USD" seq
            let result2 = Sequence.containsCurrencyCode "ABC" seq

            Assert.AreEqual(true, result1)
            Assert.AreEqual(false, result2)

            testPassed "Sequence Contains CurrencyCode Test"
        with
        | :? Npgsql.NpgsqlException as ex -> 
            testFailed "Sequence Contains CurrencyCode Test"
            printfn "%s" ex.Message

    [<Test>]
    member this.``Get Total Invoice Amount Test``() =
        try
            let seq = seq{ yield ("USD",10.00M,0.0M); 
                           yield ("USD",11.00M,0.0M);
                           yield ("USD",1530.36M,0.0M);
                           yield ("USD",750.36M,0.0M);
                           yield ("USD",71.39M,0.0M);
                           yield ("USD",82.12M,0.0M);}

            let expected = Sequence.getTotalInvoiceAmount seq

            Assert.AreEqual(2455.23M, expected)

            testPassed "Get Total Invoice Amount Test"
        with
        | :? Npgsql.NpgsqlException as ex -> 
            testFailed "`Get Total Invoice Amount Test"
            printfn "%s" ex.Message

    [<Test>]
    member this.``Get All Transactions Test``() =
        try
            let conn = Database.openConnection  
            try
                conn.Open() 
                
                let allTranasactions = QueryDatabase.getAllTransactions conn Database.getAllTransactionQueryString

                let result = allTranasactions.Count

                Assert.GreaterOrEqual(result, 80000)
            
                finally
                conn.Close()   

            testPassed "Get All Transactions Test"
        with
        | :? Npgsql.NpgsqlException as ex -> 
            testFailed "Get All Transactions Test"
            printfn "%s" ex.Message

    [<Test>]
    member this.``Get All CurrenyCodes Test``() =
        try
            let conn = Database.openConnection  
            try
                conn.Open() 
                
                let allCurrecnyCodes = QueryDatabase.getAllCurrenyCodes conn Database.getAllCurrenyCodesQueryString

                let result = allCurrecnyCodes.Count

                Assert.GreaterOrEqual(result, 38)
            
                finally
                conn.Close()   

            testPassed "Get All CurrenyCodes Test"
        with
        | :? Npgsql.NpgsqlException as ex -> 
            testFailed "Get All CurrenyCodes Test"
            printfn "%s" ex.Message

 
    member this.RunAll bool () = 
        match bool with 
        | true -> 
            printfn "%s" "Start Unit Tests"
            this.``Get Profit Margin Percentage As Decimal Test``()
            this.``Convert Invoicing Currency To Euro Test``()
            this.``Invoice CurrencyId To CurrencyNumber Test``()
            this.``Invoice CurrencyId To CurrencyCode Test``()
            this.``Encode String To Byte[] Test``()
            this.``Decode Byte[] To String Test``()
            this.``SerializeJson Test``()
            this.``DeserializeJson Test``()
            this.``Get Exchange Rates Test``()
            this.``Open Database Connection Test``()
            this.``Sequence Contains CurrencyCode Test``()
            this.``Get Total Invoice Amount Test``()
            this.``Get All Transactions Test``()
            this.``Get All CurrenyCodes Test``()
            printfn "%s" "Unit Tests Complete"

        | false ->
            printf ""