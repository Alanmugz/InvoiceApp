
namespace InvoiceApp

open Newtonsoft.Json.Linq
open System

module Json = 

    type Json =
        | JObj of Json seq
        | JProp of string * Json
        | JArr of Json seq
        | JVal of obj

    let generateJsonForInvoiceInDatabase totalInvoiceAmountPerCurrencies (messageReceived : MessageType.InvoiceMessage) 
                                         invoicingCurrencyTotalBeforeExchange invoicingCurrencyTotalAfterProfitMarginSplit 
                                         ccsProfitInEuro selectedInvoicingCurrencyCode numberOfRowsRequiredInExcelTable (invoiceNumber: string) = 
        
        let invoicingCurrencyCode = Convert.invoiceCurrencyIdToCurrencyCode messageReceived.InvoiceCurrency

        let (!!) (o: obj) = JVal o

        let rec toJson = function
            | JVal v -> new JValue(v) :> JToken
            | JProp(name, (JProp(_) as v)) -> new JProperty(name, new JObject(toJson v)) :> JToken
            | JProp(name, v) -> new JProperty(name, toJson v) :> JToken
            | JArr items -> new JArray(items |> Seq.map toJson) :> JToken
            | JObj props -> new JObject(props |> Seq.map toJson) :> JToken

        let formatItemAsJson (currencyCode, totalPerCurrencyAfterExchange, totalPerCurrencyBeforeExchange) =
            JObj [JProp("CurrencyCode", !! currencyCode);
                  JProp("TotalPerCurrencyBeforeExchange", !! totalPerCurrencyBeforeExchange); 
                  JProp("ExchangeRate", !! (Http.getExchangeRates <| invoicingCurrencyCode <| currencyCode));
                  JProp("TotalPerCurrencyAfterExchange", !! totalPerCurrencyAfterExchange)];

        let generatedJson =
            JObj [
                JProp("InvoiceNumber", !! invoiceNumber.ToString());
                JProp("InvoiceDate", !! DateTime.Now.ToShortDateString());
                JProp("InvoiceCurrency", !! invoicingCurrencyCode);
                JProp("InvoiceProfitMargin", !! messageReceived.ProfitMargin);
                JProp("InvoicePaymentCurrencyToEuroExchangeRate", !! (Http.getExchangeRates "EUR" <| invoicingCurrencyCode));
                JProp("InvoicePeroid", !! String.Format("{0} - {1}", messageReceived.DateFrom, messageReceived.DateTo.AddHours(23.0).AddMinutes(59.0).AddSeconds(59.9)));
                JProp("Transaction", 
                    JArr (Seq.map formatItemAsJson  totalInvoiceAmountPerCurrencies))
                JProp("TransactionForPeroid", !! invoicingCurrencyTotalBeforeExchange);
                JProp("InvoicingAmountWithProfitMarginApplied", !! invoicingCurrencyTotalAfterProfitMarginSplit);
                JProp("InvoicingAmountWithProfitMarginAppliedInEuro", !! ccsProfitInEuro);
            ]

        let json = toJson generatedJson 

        json

