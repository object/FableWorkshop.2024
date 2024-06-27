module App

open Elmish
open Elmish.React

open Update
open View

Program.mkProgram init update view
|> Program.withReactSynchronous "elmish-app"
|> Program.run