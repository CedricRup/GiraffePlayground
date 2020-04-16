module Giraffe.Back.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open FSharp.Data


type HeroCollection = CsvProvider<"collection.csv", InferRows = 0>
let heroes = HeroCollection.GetSample ()

type Hero =
    {
        Name : string
        Eye : string
        Alive : string
        Appearences : int Nullable
        First_Aappearence : string
        Hair : string
        Sex : string
        Alignement : string
        Secret : Guid
    }

let toHero (csvHero : HeroCollection.Row) : Hero =
    {
        Name = csvHero.NAME;
        Eye = csvHero.EYE;
        Alive = csvHero.ALIVE ;
        Appearences = csvHero.APPEARANCES;
        First_Aappearence = csvHero.``FIRST APPEARANCE``;
        Hair = csvHero.HAIR;
        Sex = csvHero.SEX;
        Alignement = csvHero.ALIGN;
        Secret = csvHero.SECRET
    }

let heroDetailHandler (name:string) =
        heroes.Rows |>
        Seq.map toHero |>
        Seq.tryFind (fun hero -> hero.Name = name) |>
        Option.map json |>
        Option.defaultValue (setStatusCode 404 >=> text "Nope")

let heroSearchByName (name:string) =         
        System.Threading.Thread.Sleep (if name.Length = 2 then 10000 else 0)
        heroes.Rows |>
        Seq.map toHero |>
        Seq.filter (fun hero -> hero.Name.Contains(name)) |>
        Seq.truncate 5 |> 
        Seq.toList |>
        function
            | [] -> setStatusCode 404 >=> text "Nope"
            | r -> json r
        

//----------------------------------
// Default template code
// ---------------------------------
// Models
// ---------------------------------

type Message =
    {
        Text : string
    }

// ---------------------------------
// Views
// ---------------------------------

module Views =
    open GiraffeViewEngine

    let layout (content: XmlNode list) =
        html [] [
            head [] [
                title []  [ encodedText "giraffe" ]
                link [ _rel  "stylesheet"
                       _type "text/css"
                       _href "/main.css" ]
            ]
            body [] content
        ]

    let partial () =
        h1 [] [ encodedText "giraffe" ]

    let index (model : Message) =
        [
            partial()
            p [] [ encodedText model.Text ]
        ] |> layout

// ---------------------------------
// Web app
// ---------------------------------

let indexHandler (name : string) =
    let greetings = sprintf "Hello %s, from Giraffe!" name
    let model     = { Text = greetings }
    let view      = Views.index model
    htmlView view



let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> indexHandler "world"
                routeCif "/hello/%s" indexHandler
                routeCif "/api/hero/%s" heroDetailHandler
                routeCif "/api/searchByName/%s" heroSearchByName
            ]
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:8080")
           .AllowAnyMethod()
           .AllowAnyHeader()
           |> ignore

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()
    (match env.IsDevelopment() with
    | true  -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler errorHandler)
        .UseHttpsRedirection()
        .UseCors(configureCors)
        .UseStaticFiles()
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddFilter(fun l -> l.Equals LogLevel.Error)
           .AddConsole()
           .AddDebug() |> ignore

[<EntryPoint>]
let main _ =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(contentRoot)
        .UseIISIntegration()
        .UseWebRoot(webRoot)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0