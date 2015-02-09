
namespace InvoiceApp
 
open fszmq
open fszmq.Context
open fszmq.Socket
open System

module ZeroMQServer =

    let server () =
      // create a ZMQ context
      use context = new Context()
  
      // create reply socket
      use server  = rep context
      // begin receiving connections
      bind server "tcp://*:5555"
  
      let rec loop () =
        // process request (i.e. 'recv' a message from our 'server')
        // NOTE: it's convenient to 'decode' the (binary) message into a string
        match server |> recv |> Utilities.decode  with
        | "hello"   ->  // valid request; send a reply back
                        // NOTE: "..."B is short-hand for a byte array of ASCII-encoded chars
                        "world"B |>> server
                        // wait for next request
                        loop() 
        | _         ->  // invalid request; stop receiving connections
                        "goodbyeFUCKER"B |>> server 

      // wait for next request
      loop () 
