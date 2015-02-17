
namespace InvoiceApp

open Npgsql
open System
open System.Collections.Generic
open System.Linq

module QueryDatabase =
    
    let private getAllTransactions (connection: NpgsqlConnection) (queryString: string) =
        let transactionLists = new List<Entity.Transcation>()
        let command = new NpgsqlCommand(queryString, connection)
        let dataReader = command.ExecuteReader()
        
        while dataReader.Read() do
            let transaction = {Entity.Transcation.MerchantId = dataReader.GetInt32(0); 
                               Entity.Transcation.MessageTypeId = dataReader.GetInt32(1); 
                               Entity.Transcation.SaleCurrencyId = dataReader.GetInt32(2);
                               Entity.Transcation.PaymentCurrencyId = dataReader.GetInt32(3);
                               Entity.Transcation.PaymentValue = dataReader.GetDouble(4);
                               Entity.Transcation.PaymentMarginValue = dataReader.GetDouble(5);
                               Entity.Transcation.CreationTimeStamp = dataReader.GetDateTime(6)}
            transactionLists.Add(transaction)
        transactionLists

    let private getAllCurrenyCodes (connection: NpgsqlConnection) (queryString: string) =
        let currencyCodeList = new List<Entity.Currency>()
        let command = new NpgsqlCommand(queryString, connection)
        let dataReader = command.ExecuteReader()
        
        while dataReader.Read() do
            let currencyCode = {Entity.Currency.Id = dataReader.GetInt32(0); 
                                Entity.Currency.Code = dataReader.GetString(1);}
            currencyCodeList.Add(currencyCode)
        currencyCodeList
        
    let getAllTransaction (messageReceived : MessageType.InvoiceMessage) () =
        
        let conn = Database.openConnection
        conn.Open()

        try
            let currencyCodes = getAllCurrenyCodes <| conn <| Database.getAllCurrenyCodesQueryString
            let transactions = getAllTransactions <| conn <| Database.getAllTransactionQueryString

            let selectedInvoicingCurrencyCode = Convert.invoiceCurrencyIdToCurrencyCode messageReceived.InvoiceCurrency

            let getTotalInvoiceAmountPerCurrencies =
                query { for transaction in transactions do 
                        join currency in currencyCodes on
                              (transaction.SaleCurrencyId = currency.Id)
                        where (transaction.MessageTypeId = 9)
                        where (transaction.MerchantId = messageReceived.MerchantId)
                        where (transaction.CreationTimeStamp >= messageReceived.DateFrom)
                        where (transaction.CreationTimeStamp <= messageReceived.DateTo.AddHours(23.0).AddMinutes(59.0).AddSeconds(59.99))
                        groupBy currency.Code into getTotalInvoiceAmountPerCurrency
                        let sumByGroup =
                            query { for value in getTotalInvoiceAmountPerCurrency do
                                    let transaction, currrencyCode = value
                                    where (transaction.SaleCurrencyId <> Convert.invoiceCurrencyIdToCurrencyNum messageReceived.InvoiceCurrency)
                                    sumBy(decimal transaction.PaymentMarginValue)
                            }                                                               
                        select (getTotalInvoiceAmountPerCurrency.Key, 
                                Math.Round(sumByGroup / (Http.getExchangeRates getTotalInvoiceAmountPerCurrency.Key <| selectedInvoicingCurrencyCode), 2),
                                sumByGroup)
                }
            
            Console.displayData getTotalInvoiceAmountPerCurrencies selectedInvoicingCurrencyCode messageReceived.ProfitMargin false ()

            Excel.generateInvoice getTotalInvoiceAmountPerCurrencies messageReceived

        finally
            conn.Close()

