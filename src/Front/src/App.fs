module App

open Fable.Core
open Thoth.Json
open Browser.Dom
open Thoth.Fetch
open System

// Mutable variable to count the number of times we clicked the button
let mutable count = 0

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

let searchHeroByName (query: string) : JS.Promise<Hero list> =
    promise {
        let url = sprintf "https://localhost:5001/api/searchByName/%s" query
        return! Fetch.get(url, caseStrategy = CamelCase)
    }

input.oninput <- fun e ->
    let resultDiv = document.querySelector("#hero-detail") :?> Browser.Types.HTMLDivElement

    promise {
        let! hero = searchHeroByName input.value
        resultDiv.innerText <- sprintf "%A" hero
    }
    |> Promise.catchEnd (fun _ -> resultDiv.innerText <- "Error")

// Register our listener
myButton.onclick <- fun _ ->
    let input = document.querySelector("#name") :?> Browser.Types.HTMLInputElement
    let resultDiv = document.querySelector("#hero-detail") :?> Browser.Types.HTMLDivElement

    promise {
        let! hero = getHeroByName input.value
        resultDiv.innerText <- sprintf "%A" hero
    }
    |> Promise.catchEnd (fun _ -> resultDiv.innerText <- "Error")
    
   // |> Promise.eitherEnd (fun r ->
   //  myButton.innerText <- sprintf "You clicked: %s time(s)" r.Name) (fun _ -> myButton.innerText <- "Error" )

