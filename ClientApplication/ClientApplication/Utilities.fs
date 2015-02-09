
namespace InvoiceApp
 
open System
open System.Text

module Utilities = 
   
    let encode messageAsStr = 
      Encoding.ASCII.GetBytes(messageAsStr.ToString())

    let decode messageAsByte = 
      Encoding.ASCII.GetString(messageAsByte)
