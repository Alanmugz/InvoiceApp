

namespace InvoiceApp

open FSharp.Data
open HttpClient

module Http = 

    type private Sample = JsonProvider<""" {"to": "EUR", "rate": 0.88111099999999998, "from": "USD"} """>

    let getExchangeRates convertFrom convertTo =
        let url = "http://rate-exchange.appspot.com/currency?from=" + convertTo + "&to=" +  convertFrom
        let page = (createRequest Get url |> getResponseBody)
        let parsedRates = Sample.Parse(page)
        parsedRates.Rate

