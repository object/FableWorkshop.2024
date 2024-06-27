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
