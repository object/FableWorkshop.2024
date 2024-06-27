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
