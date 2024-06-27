module App

open Elmish
open Elmish.React
open Elmish.Bridge

open Update
open View

Program.mkProgram init update view
|> Program.withBridgeConfig (
    Bridge.endpoint "ws://localhost:5000/socket"     
    |> Bridge.withUrlMode Raw
    |> Bridge.withMapping (fun (x : Shared.Sockets.ClientMessage) -> x |> Messages.MediaSetEvent))
|> Program.withReactSynchronous "elmish-app"
|> Program.run