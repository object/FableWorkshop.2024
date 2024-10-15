# Real-time event visualization using F# and Fable

## Prerequisites
1. Visual Studio Code (running on Windows, Linux or MacOS)
2. Ionide F# plugin (Ionide-fsharp) by Ionide
3. Basic understanding of F#
4. .NET Core version 8.0 or later
5. npm JavaScript package manager

## Recommended tools and resources
1. [The Elmish Book](https://zaid-ajaj.github.io/the-elmish-book) by Zaid Ajaj (its sample is outdated and currently doesn't build with latest Fable packages but it it still a valuable resource that explains Fable/Elmish concepts)
2. [An Introduction to Elm](https://guide.elm-lang.org/)
3. REST Client plugin by Huachao Mao
4. Chrome browser
5. Redux DevTools Chrome extension (remember to enable it)

## Workshop plan
1. Scaffolding a project with SAFE template
2. Serving the content of a file from a Saturn Web service
3. Retrieving file content using HTTP requests in a Fable app
4. Implementing playback of individual lines of a file
5. Replacing direct calls to Fable.React with Feliz
6. Adding Bulma CSS and FontAwesome to a Fable app (using CSS F# type provider)
7. From file lines player to events player (parsing text lines with Thoth.Json decoder)
8. Implementing event subscriptions using Web sockets (and Elmish.Bridge)
9. Adding live tiles (presentation of state changes)
10. Using Redux DevTools with Fable applications

## 1. Scaffolding a project with SAFE template
Now that we verified out tools, let's create an empty Fable project that we will incrementally enhance with features we need. The easiest is to use one of available scaffolding templates, and we will use SAFE template. Read more about SAFE Stack [here](https://safe-stack.github.io/). SAFE stands for Saturn-Azure-Fable-Elmish, but we will only be using SFE from its stack.

First install SAFE templates:
```
dotnet new -i SAFE.Template
```
Now generate a new Saturn/Fable/Elmish project with minimal number of features (option "-m"):
```
dotnet new SAFE -m -o FableWorkshop
```
This will create a new folder FableWorkshop with project files. Checkout its README.md file and the `src` folder. The source folder contains three subfolders: `Client`, `Server` and `Shared`.

Let's build the server first. Open the project folder in Visual Studio Code, Start the Terminal (you can start two Terminal instances: for server and client), and do the following:
```
cd src/Server
dotnet run
```
The server should be up and running after the built. Now it's time to build the client (from a different Terminal window):
```
npm install
dotnet tool restore
dotnet fable watch src/Client --run npm run start
```
After the client application is built, go to http://localhost:8080 and you should see a welcome message from SAFE.

### Inspect the project files
Browse project files, all essential client code resides in `Client/Index.fs` while the whole server is implemented in `Server/Server.fs`. The `Shared` folder contains definitions that are common for both applications.

## 2. Serving the content of a file from a Saturn Web service
We will now modify our Server app to serve file content. First, open `src/Shared/Shared.fs` file and add a new route to module `Route` so it will look like this:
```
module Route =
    let hello = "/api/hello"
    let files = "/api/files"
```
Now we need to modify our server. Replace the content of `src/Server/Server.fs` with the one from this [gist](https://gist.github.com/object/72c91ecbc2f82b6402d231a61d46fdea). The code refers ThothSerializer so we need to add its Nuget package to the project:
```
dotnet add src/Server/Server.fsproj package Thoth.Json.Giraffe
```

Build and run the app. Navigate to http://localhost:5000/api/files in the browser. Now it only shows an empty list. We will create a couple of files that we will later use in the project and place them in server's public folder. [This gist](https://gist.github.com/object/e77cfc2a1956b318dcff60a4bdb9db5c) contains files `SingleProgram.txt` and `MultiplePrograms.txt`. Create files with these names in `src/Server/public` folder and paste the respective content. Go to http://localhost:5000/api/files/SingleProgram.txt, you should get the content of a file.

### Further reading
To learn more of Saturn framework, check its [Web site](https://saturnframework.org/tutorials/how-to-start.html). [SAFE Dojo project](https://github.com/CompositionalIT/SAFE-Dojo) is a great example of how it can be used to develop Web applications and services in F#.

## 3. Retrieving file content using HTTP requests in a Fable app
To teach our Fable app how to execute HTTP requests we first need to add a new Nuget package to it. Run the following command from a Terminal:
```
dotnet add src/Client/Client.fsproj package Fable.SimpleHttp
```
While we are adding packages we will ensure that packages added by SAFE template are up-to-date, so run the following commands:
```
dotnet add src/Client/Client.fsproj package Fable.Core
dotnet add src/Client/Client.fsproj package Fable.Browser.DOM
dotnet add src/Client/Client.fsproj package Fable.Fetch
```
To display the content of HTTP responses we will need to add a view with some HTML. Fable has several viable options, in this workshop we will be using React. So we need to add a few more packages:
```
dotnet add src/Client/Client.fsproj package Fable.React
dotnet add src/Client/Client.fsproj package Fable.Elmish
dotnet add src/Client/Client.fsproj package Fable.Elmish.React
```
Finally, we will be using Thoth serializer (that has already been added to the Server app), so one more package to add:
```
dotnet add src/Client/Client.fsproj package Thoth.Fetch
```

Remember that our application uses two sets of packages: F# and JavaScript (NPM) libraries. So if we added dependency on React to our F# UI code, we need to update NPM packages accordingly:
```
npm add react
npm add react-dom
```

We can no longer fit all application code in one file, so we will split `Client.fs` into `App.fs` and `Index.fs`. Edit Client project file so it now contains the following ItemGroup:
```
    <ItemGroup>
        <None Include="index.html" />
        <Compile Include="Index.fs" />
        <Compile Include="App.fs" />
        <TypeScriptCompile Include="vite.config.mts" />
    </ItemGroup>
```

Edit `src/Client/Index.html` and replace reference to `client.fs.js` with `app.fs.js` (which contains an entry point to our application).

Now add a new F# source file `src/Client/Index.fs` with the content of this [gist](https://gist.github.com/object/b6846e1881e058e017b12d16256fff4e). Rename `Client.fs` and paste the following content (or copy from this [gist](https://gist.github.com/object/6d9d91a64ba784fad55b8efae6924822)):

```
module App

open Elmish
open Elmish.React

Program.mkProgram Index.init Index.update Index.view
|> Program.withReactSynchronous "elmish-app"
|> Program.run
```

Start both the server and client, write `SingleProgram.txt` and chances are big that you will not get a result. If you open Chrome DevTools console, you will see the following message:

>Access to XMLHttpRequest at 'http://localhost:5000/SingleProgram.txt' from origin 'http://localhost:8080' has been blocked by CORS policy: No 'Access-Control-Allow-Origin' header is present on the requested resource.

To resolve the error, import Microsoft.AspNetCore.Cors.Infrastructure namespace in `src/Server/Server.fs`:
```
open Microsoft.AspNetCore.Cors.Infrastructure
```
Add `configureCors` function after `webApp` definition:
```
let configureCors (builder : CorsPolicyBuilder) =
    builder
        .WithOrigins("http://localhost:8080")
        .AllowAnyMethod()
        .AllowAnyHeader()
    |> ignore
```
Finally call it in the `application` computational expression, so the `app` declaration will look like this:
```
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
```
Now you should be able to see the content of the file in your Fable app.

### Inspect how asynchronous HTTP requests are handled in Fable Elmish
While the Fable app is so small with most of its implementation fitting in a single file `Index.fs` it's easy to grasp and idea of underlying Elm architecture. Check the function signatures of `init`, `update` and `view`, also use of `async` computational expression in `loadEvents`. You can read more about semantics of `Cmd.OfAsync.either`, `Cmd.OfAsync.perform`, `Cmd.OfAsync.attempt` and `Cmd.OfAsync.result` in [this StackOverflow discussion](https://stackoverflow.com/questions/57619894/return-async-value-from-fable-remoting).

## 4. Implementing playback of individual lines of a file
The funcionality of your Fable app will get more complex and will be too big for a single `Index.fs` file. It's time to place model types, message types, event handler and HTML generation in separate modules.

Create new files in `src/Client` folder: `Model.fs`, `Messages.fs`, `Update.fs` and `View.fs`. Edit `src/Client/Client.fsproj` file, remote `Index.fs` and add references to these files. Remember that file order is important in F# projects, so make sure they are added in the following order:
```
    <ItemGroup>
        <None Include="index.html" />
        <Compile Include="Model.fs" />
        <Compile Include="Messages.fs" />
        <Compile Include="Update.fs" />
        <Compile Include="View.fs" />
        <Compile Include="App.fs" />
        <TypeScriptCompile Include="vite.config.mts" />
    </ItemGroup>
```
Fill the content of the newly added files from the following gists:
- [Model.fs](https://gist.github.com/object/3fbf9d8351537aac6caeed18900acaed)
- [Messages.fs](https://gist.github.com/object/9bc548b22bd9a968638a97c7a63769a4)
- [Update.fs](https://gist.github.com/object/1f25eca7af9d64c1a00f7d950e4ba704)
- [View.fs](https://gist.github.com/object/f6fa38fbda41efab1c1c86db8b698a2a)

You should also edit App.fs to open `Update` and `View` modules and modify `mkProgram` arguments (or copy code from this [gist](https://gist.github.com/object/55836c579f13d81e6ab915b1c7b142de)):
```
open Update
open View
...
Program.mkProgram init update view
```
Most likely you won't need to reload the project and Ionide plugin will catch up with the new project structure. If not, execute `Reload Window` command in Visual Studio Code.

### Inspect execution of delayed messages
The major change from the previous implementation is that upload loading a file it's split into individual lines which are asynchronously sent to the `update` message handler. Here's the essential code to achive that:
```
let delayMessage msg delay state =
    let delayedMsg (dispatch : Msg -> unit) : unit =
      let delayedDispatch = async {
        do! Async.Sleep delay
        dispatch msg
      }
      Async.StartImmediate delayedDispatch
    state, Cmd.ofEffect delayedMsg
```
Note use of a new command `Cmd.ofEffect` that enables use of subscriptions to future messages.

## 5. Replacing direct calls to Fable.React with Feliz
This stage is a relatively minor refactoring to improve the `view` implementation code style (opinion detected).

Add a reference to Feliz package:
```
dotnet add src/Client/Client.fsproj package Feliz
```
Replace `src/Client/View.fs` with an [updated version](https://gist.github.com/object/4d63244768664579ca67982bb92474a1) based on Feliz.

### Further reading
Read a great comparison by Maxime Mangel [My journey with Feliz | A comparison between Fable.React and Feliz](https://github.com/Zaid-Ajaj/Feliz/issues/155)

## 6. Adding Bulma CSS and FontAwesome to a Fable app
Getting bored of how your Fable app looks? Time to make it nicer.

Until now we didn't have to worry about `index.html` in `src/Client` folder, but this is where we import CSS, so now it's time. Open it and remove a line with a reference to `favicon.png` that will no longer be used, you can also delete the icon file from `src/Client`. Now add the following lines after `meta` element (just before the closing `head` tag):
```
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/bulma/0.9.0/css/bulma.min.css"/>
    <script src="https://kit.fontawesome.com/409fb4cc7a.js" crossorigin="anonymous"></script>
```
You will find the content of the updated `index.html` in this [gist](https://gist.github.com/object/8ca4220cc0bb20b03e2e34259a3c2ee7)
Accessing CSS element by writing magic strings is not in the mood of an F# application, so we will add CSS F# type provider to generate types for CSS elements. First add two Nuget packages to the Client project:
```
dotnet add src/Client/Client.fsproj package Zanaptak.TypedCssClasses
dotnet add src/Client/Client.fsproj package Feliz.Bulma
```
Add a new file `Styles.fs` in `src/Client` folder with the following content:
```
[<AutoOpen>]
module AppStyles

open Zanaptak.TypedCssClasses

// Bulma classes
type Bulma = CssClasses<"https://cdnjs.cloudflare.com/ajax/libs/bulma/0.9.0/css/bulma.min.css", Naming.PascalCase>

// Font-Awesome classes
type FA = CssClasses<"https://stackpath.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css", Naming.PascalCase>
```
Add a reference to Styles.fs to a project file src/Client/Client.fsproj right after the reference to `index.html` so it will be included before other F# source files:
```
<Compile Include="Styles.fs" />
```
Modify file `src/Client/Messages.fs` by replacing `Msg` case `FilenameChanged of string` with `EventSetChanged of string`, so the new definition looks like this:
```
type Msg =
    | EventSetChanged of string
    | PlaybackDelayChanged of string
    | StartPlayback
    | PausePlayback
    | StopPlayback
    | NextEvent
    | EventsLoaded of string
    | EventsError of exn
    | Delayed of Msg * delay:int
```
Replace `Client` `Model`, `Update` and `View` module files with the following files:
- [Model.fs](https://gist.github.com/object/7dfa33bf17be3638d06b0a008c648f44)
- [Update.fs](https://gist.github.com/object/e7b6253b08babda208f2f04e954f1e80)
- [View.fs](https://gist.github.com/object/89ac556fbca438df6c97afb279b2a05d)

Now you should see a CSS-powered version of the Client app that uses Bulma CSS and FontAwesome.

### Further reading
Bulma has an [excellent documentation](https://bulma.io/documentation/) with various examples. [Font Awesome](https://fontawesome.com/) has a free version that we are using for our Fable app but also a much larger paid set of fonts. And if for some reasons you are not satisfied with using Bulma in combination with F# CSS type provider, there is a Bulma F# wrapper called [Fulma](https://fulma.github.io/Fulma/) written and maintained by Maxime Mangel.

## 7. From file lines player to events player
Now that we have equipped ourselves with a great CSS and awesome fonts, we can display the content of files as what it actually represents - a series of domain events recorded while uploading media files to CDN providers. This is the important step on the way to typed communication betwen client and server.

We begin with extending the content of `src/Shared` with a new module `Dto`. The revised version can be found in [this gist](https://gist.github.com/object/765023f6441cca8ac5eb30e7148286e8).

You may need to run `dotnet fable src/Client --noCache` to ensure `Shared` project is compiled and reload the projects in Visual Studio Code to make Ionide catch up with the changes.

Now update `src/Client/Model.fs` by importing `Shared` namespace and changing type of `Events` property from string array to `Dto.MediaSetEvent array` so the `Model` type looks like this:
```
type Model = 
    { EventSet: EventSet 
      PlaybackDelay : int
      IsPlaying : bool
      Events : Dto.MediaSetEvent array
      EventIndex : int
      Error : string }
    static member Empty = 
        { EventSet = EventSet.Small
          PlaybackDelay = 2000
          IsPlaying = false
          Events = Array.empty
          EventIndex = -1
          Error = "" }
```
Both `Update` and `View` client modules need to new revisions too. They can be updated from [`Update.fs`](https://gist.github.com/object/dbcaa2b4a093ca495d82916ad969b0de) and [`View.fs`](https://gist.github.com/object/6008f718c486eb409a5463b321837c6e) gists.

### Checkout the essential changes
The most important change to `Update` module is that the text strings that earlier were displayed directly are now parsed and filtered by `MediaSetEvent` type:
```
let selectFileAndSubtitlesEvents activities =
    activities
    |> Seq.map (Decode.Auto.fromString<Dto.Activity>)
    |> Seq.choose (fun x -> match x with Ok (Dto.Activity.MediaSetEvent x) -> Some x | _ -> None)
    |> Seq.choose (fun x -> match x with | Dto.StatusUpdate _ -> None | _ -> Some x)
```
With that in place it is now possible to proper format `MediaSetEvent` rows in the `View` `showEvent` function:
```
Html.tr [
    Html.td [prop.className color; prop.children [icon]]
    Html.td [prop.className color; prop.text (mediaSetId.ToUpper())]
    Html.td [prop.className color; prop.text (provider.Substring(0,1))]
    Html.td [prop.className color; prop.text (quality)]
    Html.td [prop.className color; prop.text (timestamp.ToString(TimeFormatString))]
]
```
But we are still cheating: event data are still loaded at once from a text file instead of subscribing to them in the `Server` app. This will be taken care of in the next stage.

## 8. Implementing event subscriptions using Web sockets
It's been a long since we needed to change anything in the `Server` project, but the change is coming and the change is big: adding Web socket support.

Our server needs to new Nuget packages. So run the following commands:
```
dotnet add src/Server/Server.fsproj package Elmish.Bridge.Giraffe
dotnet add src/Server/Server.fsproj package Thoth.Json.Net
```

The `src/Shared/Shared.fs` is still growing. There will be a new route:
```
module Route =
    let hello = "/api/hello"
    let files = "/api/files"
    let socket = "/socket"
```
And a new module `Sockets` with definitions of commands sent from the Client to Server app:
```
module Sockets =
    type ClientCommand =
        | Connect
        | LoadMessages of filename:string
        | SetPlaybackDelay of int
        | StartPlayback
        | PausePlayback
        | StopPlayback

    type ClientMessage = Dto.Activity
```
Full version of `src/Shared/Shared.fs` can be downloaded [here](https://gist.github.com/object/3023171d7f5def72b5d43675f6812a0a).

We will also split `Server.fs` into `WebServer.fs` and `SocketServer.fs` for better separation of concerns. First rename `src/Server/Server.fs` to `src/Server/WebServer.fs`. Then add a file `src/Server/SocketServer.fs`. Copy the content of these files from the following gists:
- [WebServer.fs](https://gist.github.com/object/2699c9aabc7dbae335951a225b645ea9)
- [SocketServer.fs](https://gist.github.com/object/a0d5b1ab47f12debedfc23a5c6017aba)

Now edit the content of `src/Server/Server.fsproj` and update its source files:
```
  <ItemGroup>
    <Compile Include="SocketServer.fs" />
    <Compile Include="WebServer.fs" />
  </ItemGroup>
```
Now add new Nuget package to the Client app:
```
dotnet add src/Client/Client.fsproj package Elmish.Bridge.Client
```
You may need to reload project window to synchronize changes with Ionide.

Time to update Client code. Begin with `App` module by importing `Elmish.Bridge` namespace and adding a reference to Elmish Bridge after the `mkProgram` call:
```
Program.mkProgram init update view
|> Program.withBridgeConfig (
    Bridge.endpoint "ws://localhost:5000/socket"     
    |> Bridge.withUrlMode Raw
    |> Bridge.withMapping (fun (x : Shared.Sockets.ClientMessage) -> x |> Messages.MediaSetEvent))
```
Full version of `App.fs` can be obtained from [this gist](https://gist.github.com/object/9c2344d8223011129797e2feba62d236).

Other Client modules also need big revisions because we will no longer read whole files from the Server: all Server events will be sent one by one via Web socket channel. Replace other Client files with updated versions:
- [Model.fs](https://gist.github.com/object/2ccdcfd3c8e6262b9b767a4eacc85ab5)
- [Messages.fs](https://gist.github.com/object/6e821c66172448927a22b5cfde5441a1)
- [Update.fs](https://gist.github.com/object/416c49fb05291857293977b254e36a36)
- [View.fs](https://gist.github.com/object/1b380082eeaef72b1dc96b26e55c4e93)

Run both Server and Client, and you should see Server events displayed in the Client app.

### Further reading
`Elmish.Bridge` also supports broadcasting messages to all connected socket clients using `ServerHub`. Refer to its [documentation](https://github.com/Nhowka/Elmish.Bridge) for more details.

## 9. Adding live tiles
While displaying live events using Web socket is sufficient to demonstrate how subscriptions work in an Elmish application, we can improve the quality of visual presentation of server activities by adding representation of some stateful data. We will extend the Client UI with live tiles that show states of media sets being uploaded.

The Server and Shared projects are done - all DTO and sockets messages are already defined. We need to teach the Client how to use `MediaSetState` information, until now it has only been showing `MediaSetEvent` data.

We will begin with `Msg` type in `Messages` module. Rename `MediaSetEvent` case to `RemoteEvent` to generalize the name (use F2 to rename all occurences in the project). Make sure that it is renamed in `App.fs`, so `Elmish.Bridge` mapping looks like this:
```
|> Bridge.withMapping (fun (x : Shared.Sockets.ClientMessage) -> x |> Messages.RemoteEvent))
```
Add a new message case to `Msg` after `RemoteEvent`. This one will be used to implement blinking of tiles before they disappear from the dashboard:
```
    | RemoveCountdown of string
```
Client's Model, Update and View modules require bigger changes, so it's easier to download complete gists:
- [Model.fs](https://gist.github.com/object/da015a1a117d290a175eaed9924cf7ec)
- [Update.fs](https://gist.github.com/object/48f964379a910825a1a17fe312f6a407)
- [View.fs](https://gist.github.com/object/6de266a5fad672b83e65e64ebbed87d2)

Rebuild the client application (the server is probably already running) and you can monitor state changes of TV and Radio media sets.

## 10. Enable Elmish debugger using Redux DevTools (optional step)

If you get stuck with an error, it may help to check out internal state of the application. Thanks to Fable compiler that converts F# code to JavaScript, you can use Redux DevTools Chrome extension and even its time-travelling debugger to inspect the state of your Fable Elmish application and playback its activities. First, execute from the Terminal the following commands:

```
npm add remotedev -D
dotnet add src/Client/Client.fsproj package Fable.Elmish.Debugger
```

Now edit `App.fs`:

After the line that imports `Elmish.React` namespace add the line `open Elmish.Debug`
After the line that configures `Program.withReactSynchronous` add the line: `|> Program.withDebugger`

Edit `index.html` and after the line that specifies a `div` with `elmish-app` add the following script:
```
<script type="text/javascript">
    var global = global || window;
</script>
```

Restart the client application. Go the Web page, press F12 to activate Chrome development tools. Find among its tabs a tab called "Redux", Switch between tabs `Action` and `State`, and you should see messages handled by the application as defined in `Messages.fs` file and application state with properties as defined in `Model.fs`.


### Further reading
Refer to [this](https://elmish.github.io/debugger/) article to learn more about Elmish debugger. [This page](https://github.com/zalmoxisus/remotedev) contains information about RemoteDev and Redux development tools.
