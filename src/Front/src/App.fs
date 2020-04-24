module App

open Fable.Core
open Thoth.Json
open Browser.Dom
open Thoth.Fetch
open System
open FSharp.Control
open Fable.Reaction

// Get a reference to our button and cast the Element to an HTMLButtonElement
let myButton = document.querySelector(".my-button") :?> Browser.Types.HTMLButtonElement
let input = document.querySelector("#name") :?> Browser.Types.HTMLInputElement

type Hero =
    {
        Name : string
        Eye : string
        Alive : string
      //  Appearences : int Nullable
        First_Aappearence : string
        Hair : string
        Sex : string
        Alignement : string
        Secret : Guid
    }

let getHeroByName (id : string) : JS.Promise<Hero> =
    promise {
        let url = sprintf "https://localhost:5001/api/hero/%s" id
        return! Fetch.get(url, caseStrategy = CamelCase)
    }

let ofPromise (pr: Fable.Core.JS.Promise<_>) =
        AsyncRx.ofAsyncWorker(fun obv _ -> async {
            try
                let! result = Async.AwaitPromise pr
                do! obv.OnNextAsync result
                do! obv.OnCompletedAsync ()
            with
            | ex ->
                do! obv.OnErrorAsync ex
        })

let searchHeroByName (query: string) : JS.Promise<Hero list> =
    promise {
        let url = sprintf "https://localhost:5001/api/searchByName/%s" query
        return! Fetch.get(url, caseStrategy = CamelCase)
    }

let resultDiv = document.querySelector("#hero-detail") :?> Browser.Types.HTMLDivElement

let test =
    AsyncRx.ofEvent "oninput" 
    |> AsyncRx.debounce 300
    |> AsyncRx.distinctUntilChanged
    |> AsyncRx.flatMap (searchHeroByName >> ofPromise)

let observer  = AsyncObserver.Create (fun notif -> async { resultDiv.innerText <- sprintf "%A" notif }) 

test.RunAsync observer |> ignore
    
// Register our listener
myButton.onclick <- fun _ ->
    let input = document.querySelector("#name") :?> Browser.Types.HTMLInputElement
    let resultDiv = document.querySelector("#hero-detail") :?> Browser.Types.HTMLDivElement

    promise {
        let! hero = getHeroByName input.value
        resultDiv.innerText <- sprintf "%A" hero
    }
    |> Promise.catchEnd (fun _ -> resultDiv.innerText <- "Error")
