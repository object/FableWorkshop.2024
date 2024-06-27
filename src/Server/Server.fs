module Server

open System.IO
open Microsoft.AspNetCore.Cors.Infrastructure
open Giraffe
open Saturn

open Shared

let getFilesDirectory () =
    Path.Combine [|Directory.GetCurrentDirectory (); "public"|]

let getFiles ctx =
    Directory.EnumerateFiles (getFilesDirectory ())
    |> Seq.map Path.GetFileName
    |> Seq.toArray
    |> Controller.json ctx

let getFile ctx fileName =
    let filePath = Path.Combine [|getFilesDirectory (); fileName|]
    if File.Exists filePath then
        filePath |> Controller.file ctx
    else
        fileName |> Response.notFound ctx

let fileController = controller {
    index getFiles
    show getFile
}

let webApp =
    router {
        get Route.hello (json "Hello from SAFE!")
        forward Route.files fileController
    }

let configureCors (builder : CorsPolicyBuilder) =
    builder
        .WithOrigins("http://localhost:8080")
        .AllowAnyMethod()
        .AllowAnyHeader()
    |> ignore

let app =
    application {
        url "http://0.0.0.0:5000"
        use_router webApp
        memory_cache
        use_static "public"
        use_json_serializer (Thoth.Json.Giraffe.ThothSerializer())
        use_gzip
        use_cors "CORS_policy" configureCors
    }

run app