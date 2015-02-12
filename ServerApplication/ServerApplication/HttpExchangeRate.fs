

namespace InvoiceApp

module HttpRates = 
    
open System
open HttpClient
open FSharp.Data

type Simple = JsonProvider<""" {"to": "EUR", "rate": 0.88111099999999998, "from": "USD"} """>

let getRates convertFrom convertTo : decimal =
    let url = "http://rate-exchange.appspot.com/currency?from=" + convertFrom + "&to=" +  convertTo
    let page = (createRequest Get url |> getResponseBody)
    let parsedRates = Simple.Parse(page)
    parsedRates.Rate
